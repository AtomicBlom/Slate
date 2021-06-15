using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Slate.Networking.RabbitMQ
{
    public class RabbitMQRPCServer : IRPCServer
    {
        private readonly string _exchangeName;
        private readonly IRPCProvider _rabbitClient;
        private readonly string _rpcQueueName;

        internal RabbitMQRPCServer(string exchangeName, string queueName, IRPCProvider rabbitClient)
        {
            _exchangeName = exchangeName;
            _rabbitClient = rabbitClient;
            _rpcQueueName = queueName;
        }

        public IDisposable Serve<TRequest, TResponse>(Func<TRequest, Task<TResponse>> processor)
        {
            _rabbitClient.EnsureConnection();

            var consumer = new AsyncEventingBasicConsumer(_rabbitClient.Model);

            _rabbitClient.Model.QueueDeclare(_rpcQueueName, false, false, false, null);
            _rabbitClient.Model.QueueBind(_rpcQueueName, _exchangeName, _rpcQueueName);
            _rabbitClient.Model.BasicQos(0, 1, false);
            _rabbitClient.Model.BasicConsume(_rpcQueueName, false, consumer);

            consumer.Received += async (model, args) =>
            {
                Memory<byte> response = Array.Empty<byte>();
                var replyProps = _rabbitClient.Model.CreateBasicProperties();
                replyProps.CorrelationId = args.BasicProperties.CorrelationId;

                try
                {
                    var message = Serializer.Deserialize<TRequest>(args.Body);
                    

                    var responseObj = await processor(message);

                    await using var memoryStream = new MemoryStream();

                    Serializer.Serialize(memoryStream, responseObj);
                    response = memoryStream.GetBuffer().AsMemory(..(int)memoryStream.Length);
                }
                catch (Exception e)
                {
                    //FIXME: LOG THIS!!!
                }

                _rabbitClient.Model.BasicPublish(string.Empty, args.BasicProperties.ReplyTo, replyProps, response);
                _rabbitClient.Model.BasicAck(args.DeliveryTag, false);
            };


            return new ActionDisposable(() =>
            {
                foreach (var consumerTag in consumer.ConsumerTags)
                {
                    _rabbitClient.Model.BasicCancel(consumerTag);
                }
            });
        }
    }
}