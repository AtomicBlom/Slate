using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Serilog.Context;

namespace Slate.Networking.RabbitMQ
{
    internal class RabbitMQRPCClient : IRPCClient
    {
        private readonly ILogger _logger;
        private string _replyQueueName;

        public Dictionary<Guid, Action<ReadOnlyMemory<byte>>> PendingCalls = new();
        private readonly object _pendingCallLock = new();
        private AsyncEventingBasicConsumer? _consumer;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private bool _clientCreated = false;
        private object _lock = new();
        private readonly IRabbitSettings _rabbitSettings;
        private readonly IModel _model;

        internal RabbitMQRPCClient(string exchangeName, string queueName, IModel model, IRabbitSettings rabbitSettings, ILogger logger)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;
            _model = model;
            _rabbitSettings = rabbitSettings;
            _logger = logger.ForContext<RabbitMQRPCClient>();
        }

        private void EnsureRPCClient()
        {
            lock (_lock)
            {
                if (_clientCreated) return;
                _clientCreated = true;
            }

            var queueName = $"{_queueName}.Client.{_rabbitSettings.ClientName}.{Guid.NewGuid().ToString().Substring(24)}";
            _replyQueueName = _model.QueueDeclare(queueName).QueueName;
            _consumer = new AsyncEventingBasicConsumer(_model);
            _model.BasicQos(0,8, false);
            _consumer.Received += MessageReceived;

            _model.ExchangeDeclare(_exchangeName, ExchangeType.Direct, false, false);
            

            Task MessageReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
            {
                Action<ReadOnlyMemory<byte>> ? actionToRun = null;

                lock (_pendingCallLock)
                {
                    if (!basicDeliverEventArgs.BasicProperties.IsCorrelationIdPresent() ||
                        !Guid.TryParse(basicDeliverEventArgs.BasicProperties.CorrelationId, out var correlationId))
                    {
                        return Task.CompletedTask;
                    }

                    using var correlationContext = LogContext.PushProperty("RabbitMQCorrelationId", correlationId);

                    if (PendingCalls.TryGetValue(correlationId, out actionToRun))
                    {
                        PendingCalls.Remove(correlationId);
                    }
                }

                try
                {
                    if (actionToRun == null)
                    {
                        throw new Exception("Attempted to process a message that did not belong to us");
                    }

                    actionToRun?.Invoke(basicDeliverEventArgs.Body);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to process the callback of an RPC response");
                }

                

                return Task.CompletedTask;
            }

            _model.BasicConsume(_consumer, _replyQueueName, true);
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request)
        {
            var _logger = this._logger
                .ForContext("RabbitMQRequestType", typeof(TRequest).FullName)
                .ForContext("RabbitMQResponseType", typeof(TResponse).FullName);

            EnsureRPCClient();
            
            var props = _model.CreateBasicProperties();
            var correlationId = Guid.NewGuid();
            using var correlationContext = LogContext.PushProperty("RabbitMQCorrelationId", correlationId);
            props.CorrelationId = correlationId.ToString();
            props.ReplyTo = _replyQueueName;

            var tcs = new TaskCompletionSource<TResponse>();

            lock (_pendingCallLock)
            {
                void HandleResponse(ReadOnlyMemory<byte> memory)
                {
                    try
                    {
                        var message = Serializer.Deserialize<TResponse>(memory);
                        var logger = _rabbitSettings.IncludeMessageContentsInLogs
                            ? _logger.ForContext("MessageBody", message, true)
                            : _logger;
                        logger.Verbose("Received a message {MessageType}", typeof(TResponse).Name);

                        tcs.SetResult(message);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed to process a {MessageType} RPC Result", typeof(TRequest).Name);

                        //FIXME: Try deserialize an error message to put in an exception?
                        tcs.SetException(e);
                    }
                }

                PendingCalls.Add(correlationId, HandleResponse);
            }

            await using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, request);

            _model.BasicPublish(
                exchange: _exchangeName,
                routingKey: $"{_queueName}.{typeof(TRequest).Name}",
                basicProperties: props,
                body: memoryStream.GetBuffer().AsMemory(..(int)memoryStream.Length));

            var logger = _rabbitSettings.IncludeMessageContentsInLogs
                ? _logger.ForContext("MessageBody", request, true)
                : _logger;
            logger.Verbose("Sent a message {MessageType}", typeof(TRequest).Name);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var firstResponse = await Task.WhenAny(tcs.Task, timeoutTask);

            if (firstResponse == timeoutTask)
            {
                throw new TimeoutException();
            }

            return await tcs.Task;
        }


        public void Dispose()
        {
            if (_consumer is null) return;

            foreach (var consumerTag in _consumer.ConsumerTags)
            {
                _model.BasicCancel(consumerTag);
                
            }
        }
    }
}