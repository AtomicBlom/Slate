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
        private readonly ILogger _logger;
        private readonly string _rpcQueueName;
        private readonly IModel _model;
        private readonly IRabbitSettings _rabbitSettings;

        internal RabbitMQRPCServer(string exchangeName, string queueName, IModel model, IRabbitSettings rabbitSettings, ILogger logger)
        {
            _exchangeName = exchangeName;
            _logger = logger.ForContext<RabbitMQRPCServer>();
            _rpcQueueName = queueName;
            _model = model;
            _rabbitSettings = rabbitSettings;
        }

        public IDisposable Serve<TRequest, TResponse>(Func<TRequest, Task<TResponse>> processor)
        {
            var consumer = new AsyncEventingBasicConsumer(_model);

            _model.ExchangeDeclare(_exchangeName, ExchangeType.Direct, false, false);
            _model.QueueDeclare($"{_rpcQueueName}.{typeof(TRequest).Name}", false, false, false, null);
            _model.QueueBind($"{_rpcQueueName}.{typeof(TRequest).Name}", _exchangeName, $"{_rpcQueueName}.{typeof(TRequest).Name}");
            _model.BasicQos(0, 1, false);
            
            _logger.Information("Listening to RPC pair Request {RequestType}, Response {ResponseType}", typeof(TRequest).Name, typeof(TResponse).Name);

            consumer.Received += async (model, args) =>
            {
                Memory<byte> response = Array.Empty<byte>();
                var replyProps = _model.CreateBasicProperties();
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

                _model.BasicPublish(string.Empty, args.BasicProperties.ReplyTo, replyProps, response);
                _model.BasicAck(args.DeliveryTag, false);
            };

            _model.BasicConsume($"{_rpcQueueName}.{typeof(TRequest).Name}", false, consumer);

            return new ActionDisposable(() =>
            {
                foreach (var consumerTag in consumer.ConsumerTags)
                {
                    _model.BasicCancel(consumerTag);
                }
            });
        }
    }
}