using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Slate.Networking.RabbitMQ
{
    public class RabbitClient : IRabbitClient, IDisposable, IRPCProvider
    {
        private readonly IRabbitSettings _rabbitSettings;
        private readonly ILogger _logger;
        private bool _disposed;

        public IModel Model { get; }
        public IRabbitSettings Settings => _rabbitSettings;

        public RabbitClient(IModel model, ILogger logger, IRabbitSettings rabbitSettings)
        {
            _rabbitSettings = rabbitSettings;
            Model = model;
            _logger = logger.ForContext<RabbitClient>();
        }

        public IDisposable Subscribe<T>(Func<T, Task> action)
        {
            if (_disposed)
            {
                _logger.Warning("Attempted to send a message after the RabbitClient has been disposed");
                throw new ObjectDisposedException(nameof(RabbitClient));
            }

            Model.ExchangeDeclare(exchange: "e.Slate.Fanout", type: ExchangeType.Fanout);
            
            var queue = $"q.{typeof(T).FullName}";
            Model.QueueDeclare(queue, autoDelete: false);
            Model.QueueBind(queue, "e.Slate.Fanout", typeof(T).FullName);
            var consumer = new AsyncEventingBasicConsumer(Model);
            
            consumer.Received += ConsumeMessage;
            var consumerTag = Model.BasicConsume(queue, false, consumer);
            var subscriptionToken = new ActionDisposable(() =>
            {
                Model?.BasicCancel(consumerTag);
                consumer.Received -= ConsumeMessage;
            });

            _logger.Information("Subscribed to message {MessageType}", typeof(T).Name);


            return subscriptionToken;

            async Task ConsumeMessage(object? sender, BasicDeliverEventArgs args)
            {
                var deliveryTag = args.DeliveryTag;
                try
                {
                    var message = Serializer.Deserialize<T>(args.Body);

                    var logger = _rabbitSettings.IncludeMessageContentsInLogs
                        ? _logger.ForContext("MessageBody", message, true)
                        : _logger;
                    logger.Verbose("Received a message {MessageType}", typeof(T).Name);

                    await action(message);
                    Model?.BasicAck(deliveryTag, false);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Could not process a {MessageType} message", typeof(T).Name);
                    //FIXME: Consider whether a message can be re-queued 
                    Model?.BasicNack(deliveryTag, false, true);
                }
            }
        }

        public void Send<T>(T message)
        {
            if (_disposed)
            {
                _logger.Warning("Attempted to send a message after the RabbitClient has been disposed");
                throw new ObjectDisposedException(nameof(RabbitClient));
            }

            try
            {
                using MemoryStream ms = new();
                Serializer.Serialize(ms, message);

                Model.ExchangeDeclare("e.Slate.Fanout", ExchangeType.Fanout);
                Model.BasicPublish("e.Slate.Fanout", typeof(T).FullName, body: ms.ToArray());

                var logger = _rabbitSettings.IncludeMessageContentsInLogs
                    ? _logger.ForContext("MessageBody", message, true)
                    : _logger;
                logger.Verbose("Sent a message {MessageType}", typeof(T).Name);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not send a {MessageType} message", typeof(T));
            }
        }

        public IRPCClient CreateRPCClient()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitClient));
            return new RabbitMQRPCClient("e.Slate.RPC", "q.Slate.RPC", this, _logger);
        }

        public IRPCServer CreateRPCServer()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitClient));
            return new RabbitMQRPCServer("e.Slate.RPC", "q.Slate.RPC", this, _logger);
        }

        public void Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
            Model?.Dispose();
        }
    }
}
