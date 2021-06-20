using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Serilog.Context;

namespace Slate.Networking.RabbitMQ
{
    public class RabbitMQRPCServer : IRPCServer
    {
        private readonly string _exchangeName;
        private readonly IRPCProvider _rabbitClient;
        private readonly ILogger _logger;
        private readonly string _rpcQueueName;
        private readonly IRabbitSettings _rabbitSettings;

        internal RabbitMQRPCServer(string exchangeName, string queueName, IRPCProvider rabbitClient, ILogger logger)
        {
            _exchangeName = exchangeName;
            _rabbitClient = rabbitClient;
            _rabbitSettings = rabbitClient.Settings;
            _logger = logger.ForContext<RabbitMQRPCServer>();
            _rpcQueueName = queueName;
        }

        public IDisposable Serve<TRequest, TResponse>(Func<TRequest, Task<TResponse>> processor)
        {
            var consumer = new AsyncEventingBasicConsumer(_rabbitClient.Model);

            _rabbitClient.Model.ExchangeDeclare(_exchangeName, ExchangeType.Direct, false, false);
            _rabbitClient.Model.QueueDeclare(_rpcQueueName, false, false, false, null);
            _rabbitClient.Model.QueueBind(_rpcQueueName, _exchangeName, _rpcQueueName);
            _rabbitClient.Model.BasicQos(0, 1, false);
            _rabbitClient.Model.BasicConsume(_rpcQueueName, false, consumer);

            _logger.Information("Listening to RPC pair Request {RequestType}, Response {ResponseType}", typeof(TRequest).Name, typeof(TResponse).Name);

            consumer.Received += async (model, args) =>
            {
                Memory<byte> response = Array.Empty<byte>();
                var replyProps = _rabbitClient.Model.CreateBasicProperties();
                replyProps.CorrelationId = args.BasicProperties.CorrelationId;
                using var correlationIdContext =
                    LogContext.PushProperty("CorrelationId", args.BasicProperties.CorrelationId);

                try
                {
                    var message = Serializer.Deserialize<TRequest>(args.Body);
                    var logger = _rabbitSettings.IncludeMessageContentsInLogs
                        ? _logger.ForContext("MessageBody", message, true)
                        : _logger;
                    logger.Verbose("Received a message {MessageType}", typeof(TRequest).Name);


                    var responseObj = await processor(message);

                    await using var memoryStream = new MemoryStream();

                    Serializer.Serialize(memoryStream, responseObj);
                    response = memoryStream.GetBuffer().AsMemory(..(int)memoryStream.Length);

                    logger = _rabbitSettings.IncludeMessageContentsInLogs
                        ? _logger.ForContext("MessageBody", responseObj, true)
                        : _logger;
                    logger.Verbose("Replied to an RPC message with a {MessageType}", typeof(TResponse).Name);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing an RPC Request {MessageType}", typeof(TRequest).Name);
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