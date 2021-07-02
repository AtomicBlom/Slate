using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using MessagePipe;
using Nito.AsyncEx;
using ProtoBuf.Grpc.Client;
using Serilog;
using Slate.Backend.Shared;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Cell.Services;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.GameWarden.Game
{
    public class CellConnectionManager : ICellConnectionManager
    {
        private readonly EventFactory _eventFactory;
        private readonly IBufferedPublisher<MessageToGameWarden> _snowglobePublisher;
        private readonly ILogger _logger;
        private readonly Dictionary<Guid, Task> _knownCells = new();
        private readonly AsyncReaderWriterLock _knownCellLock = new();

        public CellConnectionManager(EventFactory eventFactory, IBufferedPublisher<MessageToGameWarden> snowglobePublisher, ILogger logger)
        {
            _eventFactory = eventFactory;
            _snowglobePublisher = snowglobePublisher;
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

            var messagesFromSnowglobe = cellService.SubscribeAsync(SendMessagesToSnowglobe(guid, cancellationToken));
            var _ = Task.Run(() => ProcessMessagesFromSnowglobe(guid, messagesFromSnowglobe, cancellationToken));
            
            tcs.SetResult();
            return tcs.Task;
        }

        private async Task ProcessMessagesFromSnowglobe(Guid instanceId, IAsyncEnumerable<MessageToGameWarden> messagesFromSnowglobe, CancellationToken cancellationToken)
        {
            using var cellInstanceContext = CommonLogContexts.ApplicationInstanceId(instanceId);
            _logger.Information("Ready to receive messages from Cell");
            await foreach (var message in messagesFromSnowglobe.WithCancellation(cancellationToken))
            {
                _snowglobePublisher.Publish(message);
            }
        }

        async IAsyncEnumerable<MessageToSnowglobe> SendMessagesToSnowglobe(Guid instanceId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var CellInstanceContext = CommonLogContexts.ApplicationInstanceId(instanceId);
            var (subscription, events) = _eventFactory
                .CreateAsyncEvent<MessageToSnowglobe>();

            cancellationToken.Register(() =>
            {
                subscription.Dispose();
            });
            
            var messages = events
                .AsAsyncEnumerable(new SnowglobeInstanceFilter(instanceId))
                .WithCancellation(cancellationToken);

            _logger.Information("Ready to send messages to Cell");
            
            await foreach (var message in messages)
            {
                if (cancellationToken.IsCancellationRequested) break;

                yield return message;
            }
        }
    }

    public class SnowglobeInstanceFilter : AsyncMessageHandlerFilter<MessageToSnowglobe>
    {
        private readonly Uuid _instanceId;

        public SnowglobeInstanceFilter(Guid instanceId)
        {
            _instanceId = instanceId.ToUuid();
        }

        public override ValueTask HandleAsync(MessageToSnowglobe message, CancellationToken cancellationToken, Func<MessageToSnowglobe, CancellationToken, ValueTask> next)
        {
            if (message.InstanceId == _instanceId)
            {
                next(message, cancellationToken);
            }
            return ValueTask.CompletedTask;
        }
    }
}