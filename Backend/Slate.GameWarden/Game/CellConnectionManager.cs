using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Grpc.Net.Client;
using Nito.AsyncEx;
using ProtoBuf.Grpc.Client;
using Serilog;
using Slate.Backend.Shared;
using Slate.Events.InMemory;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Cell.Services;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.GameWarden.Game
{
    public class CellConnectionManager : ICellConnectionManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly Dictionary<Guid, Task> _knownCells = new();
        private readonly AsyncReaderWriterLock _knownCellLock = new();
        private IDisposable bleah;

        public CellConnectionManager(IEventAggregator eventAggregator, ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger.ForContext<CellConnectionManager>();
        }

        public async Task<Task> GetOrConnectAsync(Guid guid, Endpoint endpoint)
        {
            using (await _knownCellLock.ReaderLockAsync())
            {
                if (_knownCells.TryGetValue(guid, out var connectTask))
                {
                    return connectTask;
                }
            }

            TaskCompletionSource? tcs;

            using (await _knownCellLock.WriterLockAsync())
            {
                if (_knownCells.TryGetValue(guid, out var connectTask))
                {
                    return connectTask;
                }

                tcs = new TaskCompletionSource();
                _knownCells.Add(guid, tcs.Task);
            }

            using var cellInstanceContext = CommonLogContexts.ApplicationInstanceId(guid);
            _logger.Information("Connecting to cell");

            var channel = GrpcChannel.ForAddress($"http://{endpoint.Hostname}:{endpoint.Port}", new GrpcChannelOptions
            {
                HttpClient = new HttpClient
                {
                    //FIXME: Auth between backend services?
                    //DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", authToken) }
                }
            });

            var cellService = channel.CreateGrpcService<ICellService>();

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            bleah = cancellationTokenSource;

            var sendReady = new TaskCompletionSource();
            var receiveReady = new TaskCompletionSource();

            var messagesFromSnowglobe = cellService.SubscribeAsync(SendMessagesToSnowglobe(guid, sendReady, cancellationToken));
            var _ = Task.Run(() => ProcessMessagesFromSnowglobe(guid, messagesFromSnowglobe, receiveReady, cancellationToken));

            await Task.WhenAll(sendReady.Task, receiveReady.Task);

            tcs.SetResult();
            return tcs.Task;
        }

        private async Task ProcessMessagesFromSnowglobe(
            Guid instanceId,
            IAsyncEnumerable<MessageToGameWarden> messagesFromSnowglobe, 
            TaskCompletionSource receiveReady,
            CancellationToken cancellationToken)
        {
            using var cellInstanceContext = CommonLogContexts.ApplicationInstanceId(instanceId);
            _logger.Information("Ready to receive messages from Cell");

            receiveReady.SetResult();
            await foreach (var message in messagesFromSnowglobe.WithCancellation(cancellationToken))
            {
                _eventAggregator.Publish(message);
            }
        }

        async IAsyncEnumerable<MessageToSnowglobe> SendMessagesToSnowglobe(
            Guid instanceId,
            TaskCompletionSource sendReady, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var networkInstanceId = instanceId.ToUuid();

            var buffer = new BufferBlock<MessageToSnowglobe>();
            
            using var cellInstanceContext = CommonLogContexts.ApplicationInstanceId(instanceId);

            _eventAggregator.GetEvent<MessageToSnowglobe>()
                .Where(m => m.InstanceId is null || m.InstanceId.Equals(networkInstanceId))
                .Subscribe(t => buffer.Post(t), cancellationToken);

            _logger.Information("Ready to send messages to Cell");
            sendReady.SetResult();
            
            await foreach (var message in buffer.ReceiveAllAsync(cancellationToken))
            {
                _logger.Information("Sending a {MessageType} to Snowglobe", message.GetType().FullName);

                yield return message;
            }
        }
    }
}