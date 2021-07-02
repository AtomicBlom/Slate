using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePipe;
using ProtoBuf.Grpc;
using Serilog;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Cell.Services;

namespace Slate.Snowglobe
{
    public class CellService : ICellService
    {
        private readonly IBufferedAsyncSubscriber<MessageToGameWarden> _sendMessageBus;
        private readonly IBufferedPublisher<MessageToSnowglobe> _receivedMessagesBus;
        private readonly ILogger _logger;

        public CellService(ILogger logger, IBufferedAsyncSubscriber<MessageToGameWarden> sendMessageBus, IBufferedPublisher<MessageToSnowglobe> receivedMessagesBus)
        {
            _sendMessageBus = sendMessageBus;
            _receivedMessagesBus = receivedMessagesBus;
            _logger = logger.ForContext<CellService>();
        }

        public IAsyncEnumerable<MessageToGameWarden> SubscribeAsync(IAsyncEnumerable<MessageToSnowglobe> messages, CallContext context = default)
        {
            Task.Run(async () => await ProcessMessagesFromGameWarden(messages));
            return SendMessagesFromSnowglobe();
        }

        private async IAsyncEnumerable<MessageToGameWarden> SendMessagesFromSnowglobe()
        {
            await foreach (var message in _sendMessageBus.AsAsyncEnumerable())
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
                    _receivedMessagesBus.Publish(message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing a client message {MessageType}", message.GetType().FullName);
                }
            }
        }
    }
}
