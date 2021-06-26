using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Slate.Networking.RabbitMQ
{
    public class RabbitClient : IRabbitClient, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IRabbitSettings _rabbitSettings;
        private readonly ILogger _logger;
        private bool _disposed;
        private IModel? _subscriptionModel;
        public IRabbitSettings Settings => _rabbitSettings;

        public RabbitClient(IConnection connection, ILogger logger, IRabbitSettings rabbitSettings)
        {
            _connection = connection;
            _rabbitSettings = rabbitSettings;
            _logger = logger.ForContext<RabbitClient>();
        }

        //FIXME: Consider what happens if this gets called asynchronously? - Introduce locking?
        public IDisposable Subscribe<T>(Func<T, Task> action, ushort parallelism = 0)
        {
            _subscriptionModel ??= _connection.CreateModel();

            try
            {
                if (_disposed)
                {
                    _logger.Warning("Attempted to send a message after the RabbitClient has been disposed");
                    throw new ObjectDisposedException(nameof(RabbitClient));
                }

                var queueName = $"q.Slate.Fanout.{_rabbitSettings.ClientName}.{Guid.NewGuid().ToString().Substring(24)}";

                _subscriptionModel.ExchangeDeclare("e.Slate.Fanout", ExchangeType.Fanout);
                _subscriptionModel.QueueDeclare(queueName);
                _subscriptionModel.QueueBind(queueName, "e.Slate.Fanout", typeof(T).FullName);
                _subscriptionModel.BasicQos(0, parallelism, false);
                var consumer = new AsyncEventingBasicConsumer(_subscriptionModel);
                consumer.Received += ConsumeMessage;
                var consumerTag = _subscriptionModel.BasicConsume(queueName, false, consumer);
                var subscriptionToken = new ActionDisposable(() =>
                {
                    _logger.Verbose("Cleaning up queue for subscription {MessageType}", typeof(T).Name);
                    _subscriptionModel?.BasicCancel(consumerTag);
                    consumer.Received -= ConsumeMessage;
                });

                _logger.Information("Subscribed to message {MessageType}", typeof(T).Name);

                return subscriptionToken;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error subscribing to message {MessageType}", typeof(T).Name);
                throw;
            }

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
                    _subscriptionModel?.BasicAck(deliveryTag, false);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Could not process a {MessageType} message", typeof(T).Name);
                    //FIXME: Consider whether a message can be re-queued 
                    _subscriptionModel?.BasicNack(deliveryTag, false, true);
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

            _subscriptionModel ??= _connection.CreateModel();

            try
            {
                using MemoryStream ms = new();
                Serializer.Serialize(ms, message);

                _subscriptionModel.ExchangeDeclare("e.Slate.Fanout", ExchangeType.Fanout);
                _subscriptionModel.BasicPublish("e.Slate.Fanout", typeof(T).FullName, body: ms.ToArray());

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
            return new RabbitMQRPCClient("e.Slate.RPC", "q.Slate.RPC", _connection.CreateModel(), _rabbitSettings, _logger);
        }

        public IRPCServer CreateRPCServer()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitClient));
            return new RabbitMQRPCServer("e.Slate.RPC", "q.Slate.RPC", _connection.CreateModel(), _rabbitSettings, _logger);
        }

        public void Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
            _subscriptionModel?.Dispose();
        }
    }
}
