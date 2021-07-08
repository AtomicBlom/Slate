using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Serilog;
using Slate.Events.InMemory;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Cell.Services;

namespace Slate.Snowglobe
{
    public class CellService : ICellService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public CellService(IEventAggregator eventAggregator, ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger.ForContext<CellService>();
        }

        public IAsyncEnumerable<MessageToGameWarden> SubscribeAsync(IAsyncEnumerable<MessageToSnowglobe> messages, CallContext context = default)
        {
            Task.Run(async () => await ProcessMessagesFromGameWarden(messages));
            return SendMessagesFromSnowglobe();
        }

        private async IAsyncEnumerable<MessageToGameWarden> SendMessagesFromSnowglobe()
        {
            var observable = _eventAggregator
                .GetEvent<MessageToGameWarden>();
            await foreach (var message in observable.ToAsyncEnumerable())
            {
                _logger.Information("Sending message {MessageType} to GameWarden", message.GetType().FullName);
                yield return message;
            }
        }

        async Task ProcessMessagesFromGameWarden(IAsyncEnumerable<MessageToSnowglobe> messages)
        {
            await foreach (var message in messages)
            {
                try
                {
                    _logger.Information("Received message {MessageType} from GameWarden", message.GetType().FullName);
                    _eventAggregator.Publish(message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing a client message {MessageType}", message.GetType().FullName);
                }
            }
        }
    }
}
