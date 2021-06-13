using System;
using System.IO;
using System.Threading.Tasks;
using Game.Networking.Internal.Protocol;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Game.Networking.Internal.Client.RabbitMQ
{
    public interface IRabbitClient
    {
        IDisposable Subscribe<T>(Action<T> action);
        void Send<T>(T message);

        Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request) 
            where TRequest : ICorrelatedObject
            where TResponse : ICorrelatedObject;
    }

    public interface ICorrelatedObject
    {
        Uuid CorrelationId { get; }
    }

    public class RabbitClient : IRabbitClient, IDisposable
    {
        private readonly IRabbitSettings _rabbitSettings;
        private readonly object _lock = new();
        private bool _connectionStarted;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitClient(IRabbitSettings rabbitSettings)
        {
            _rabbitSettings = rabbitSettings;
        }

        private void EnsureConnection()
        {
            lock (_lock)
            {
                if (_connectionStarted) return;

                var factory = new ConnectionFactory
                {
                    HostName = _rabbitSettings.Hostname,
                    Port = _rabbitSettings.Port,
                    UserName = _rabbitSettings.Username,
                    Password = _rabbitSettings.Password,
                    VirtualHost = _rabbitSettings.VirtualHost,
                    ClientProvidedName = _rabbitSettings.ClientName
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _connectionStarted = true;
            }
        }


        public IDisposable Subscribe<T>(Action<T> action)
        {
            EnsureConnection();

            _channel.ExchangeDeclare(exchange: "e.Slate.Fanout", type: ExchangeType.Fanout);


            var queue = $"q.{typeof(T).FullName}";
            _channel.QueueDeclare(queue, autoDelete: false);
            _channel.QueueBind(queue, "e.Slate.Fanout", typeof(T).FullName);
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += ConsumeMessage;
            var consumerTag = _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
            var subscriptionToken = new ActionDisposable(() =>
            {
                _channel?.BasicCancel(consumerTag);
                consumer.Received -= ConsumeMessage;
            });
            
            return subscriptionToken;

            void ConsumeMessage(object? sender, BasicDeliverEventArgs args)
            {
                var message = Serializer.Deserialize<T>(args.Body);
                var deliveryTag = args.DeliveryTag;
                try
                {
                    action(message);
                    _channel?.BasicAck(deliveryTag, false);
                }
                catch (Exception e)
                {
                    //FIXME: Log this
                    //FIXME: Consider whether a message can be re-queued 
                    _channel?.BasicNack(deliveryTag, false, true);
                }
            }
        }

        public void Send<T>(T message)
        {
            using MemoryStream ms = new();
            Serializer.Serialize(ms, message);

            _channel.ExchangeDeclare(exchange: "e.Slate.Fanout", type: ExchangeType.Fanout);
            _channel.BasicPublish("e.Slate.Fanout", typeof(T).FullName, body:ms.ToArray());
        }

        public Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request) where TRequest : ICorrelatedObject where TResponse : ICorrelatedObject
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }

    public interface IRabbitSettings
    {
        string Hostname { get; }
        string VirtualHost { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
        string ClientName { get; }
    }

    public class RabbitSettings : IRabbitSettings
    {
        public string Hostname { get; set; } = "localhost";
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ClientName { get; } = "RabbitMQ client";
    }
}
