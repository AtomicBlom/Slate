using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Slate.Networking.RabbitMQ
{
    internal interface IRPCProvider
    {
        void EnsureConnection();
        IModel? Model { get; }
    }

    public class RabbitClient : IRabbitClient, IDisposable, IRPCProvider
    {
        private readonly IRabbitSettings _rabbitSettings;
        private readonly object _lock = new();
        private bool _connectionStarted;
        private IConnection? _connection;
        public IModel? Model { get; private set; }

        public RabbitClient(IRabbitSettings rabbitSettings)
        {
            _rabbitSettings = rabbitSettings;
        }

        public void EnsureConnection()
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
                    ClientProvidedName = _rabbitSettings.ClientName,
                    DispatchConsumersAsync = true
                };
                _connection = factory.CreateConnection();
                Model = _connection.CreateModel();
                _connectionStarted = true;
            }
        }
        
        public IDisposable Subscribe<T>(Func<T, Task> action)
        {
            EnsureConnection();

            Model.ExchangeDeclare(exchange: "e.Slate.Fanout", type: ExchangeType.Fanout);


            var queue = $"q.{typeof(T).FullName}";
            Model.QueueDeclare(queue, autoDelete: false);
            Model.QueueBind(queue, "e.Slate.Fanout", typeof(T).FullName);
            var consumer = new AsyncEventingBasicConsumer(Model);
            
            consumer.Received += ConsumeMessage;
            var consumerTag = Model.BasicConsume(queue, autoAck: false, consumer: consumer);
            var subscriptionToken = new ActionDisposable(() =>
            {
                Model?.BasicCancel(consumerTag);
                consumer.Received -= ConsumeMessage;
            });
            
            return subscriptionToken;

            async Task ConsumeMessage(object? sender, BasicDeliverEventArgs args)
            {
                var message = Serializer.Deserialize<T>(args.Body);
                var deliveryTag = args.DeliveryTag;
                try
                {
                    await action(message);
                    Model?.BasicAck(deliveryTag, false);
                }
                catch (Exception e)
                {
                    //FIXME: Log this
                    //FIXME: Consider whether a message can be re-queued 
                    Model?.BasicNack(deliveryTag, false, true);
                }
            }
        }

        public void Send<T>(T message)
        {
            using MemoryStream ms = new();
            Serializer.Serialize(ms, message);

            Model.ExchangeDeclare("e.Slate.Fanout", ExchangeType.Fanout);
            Model.BasicPublish("e.Slate.Fanout", typeof(T).FullName, body:ms.ToArray());
        }

        public IRPCClient CreateRPCClient()
        {
            return new RabbitMQRPCClient("e.Slate.RPC", "q.Slate.RPC", this);
        }

        public IRPCServer CreateRPCServer()
        {
            return new RabbitMQRPCServer("e.Slate.RPC", "q.Slate.RPC", this);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _connection?.Dispose();
            Model?.Dispose();
        }
    }
}
