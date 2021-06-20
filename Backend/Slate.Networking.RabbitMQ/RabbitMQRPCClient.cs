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
        private readonly IRPCProvider _rabbitClient;
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

        internal RabbitMQRPCClient(string exchangeName, string queueName, IRPCProvider rabbitClient, ILogger logger)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;
            _rabbitClient = rabbitClient;
            _rabbitSettings = rabbitClient.Settings;
            _logger = logger.ForContext<RabbitMQRPCClient>();
        }

        private void EnsureRPCClient()
        {
            lock (_lock)
            {
                if (_clientCreated) return;
                _clientCreated = true;
            }

            _replyQueueName = _rabbitClient.Model.QueueDeclare().QueueName;
            _consumer = new AsyncEventingBasicConsumer(_rabbitClient.Model);
            _consumer.Received += MessageReceived;

            _rabbitClient.Model.ExchangeDeclare(_exchangeName, ExchangeType.Direct, false, false);

            Task MessageReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
            {
                lock (_pendingCallLock)
                {
                    if (!basicDeliverEventArgs.BasicProperties.IsCorrelationIdPresent() ||
                        !Guid.TryParse(basicDeliverEventArgs.BasicProperties.CorrelationId, out var correlationId))
                    {
                        return Task.CompletedTask;
                    }

                    using var correlationContext = LogContext.PushProperty("CorrelationId", correlationId);

                    if (PendingCalls.TryGetValue(correlationId, out var action))
                    {
                        try
                        {
                            action(basicDeliverEventArgs.Body);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Failed to process the callback of an RPC response");
                        }
                    }
                }

                return Task.CompletedTask;
            }

            _rabbitClient.Model.BasicConsume(_consumer, _replyQueueName, true);
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request)
        {
            EnsureRPCClient();
            
            var props = _rabbitClient.Model.CreateBasicProperties();
            var correlationId = Guid.NewGuid();
            using var correlationContext = LogContext.PushProperty("CorrelationId", correlationId);
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

            _rabbitClient.Model.BasicPublish(
                exchange: _exchangeName,
                routingKey: _queueName,
                basicProperties: props,
                body: memoryStream.GetBuffer().AsMemory(..(int)memoryStream.Length));

            var logger = _rabbitSettings.IncludeMessageContentsInLogs
                ? _logger.ForContext("MessageBody", request, true)
                : _logger;
            logger.Verbose("Sent a message {MessageType}", typeof(TResponse).Name);

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
            if (_consumer is null || _rabbitClient.Model is null) return;

            foreach (var consumerTag in _consumer.ConsumerTags)
            {
                _rabbitClient.Model.BasicCancel(consumerTag);
                
            }
        }
    }
}