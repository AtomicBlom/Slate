using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Slate.Networking.RabbitMQ
{
    internal class RabbitMQRPCClient : IRPCClient
    {
        private readonly IRPCProvider _rabbitClient;
        private string _replyQueueName;

        public Dictionary<Guid, Action<ReadOnlyMemory<byte>>> PendingCalls = new();
        private readonly object _pendingCallLock = new();
        private AsyncEventingBasicConsumer _consumer;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private bool _clientCreated = false;
        private object _lock = new();

        internal RabbitMQRPCClient(string exchangeName, string queueName, IRPCProvider rabbitClient)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;
            _rabbitClient = rabbitClient;
        }

        private void EnsureRPCClient()
        {
            lock (_lock)
            {
                if (_clientCreated) return;
                _clientCreated = true;
            }

            _rabbitClient.EnsureConnection();
            
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

                    if (PendingCalls.TryGetValue(correlationId, out var action))
                    {
                        try
                        {
                            action(basicDeliverEventArgs.Body);
                        }
                        catch (Exception e)
                        {
                            //FIXME: Log this!!!
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
            props.CorrelationId = correlationId.ToString();
            props.ReplyTo = _replyQueueName;

            var tcs = new TaskCompletionSource<TResponse>();

            lock (_pendingCallLock)
            {
                void CallService(ReadOnlyMemory<byte> memory)
                {
                    try
                    {
                        var response = Serializer.Deserialize<TResponse>(memory);
                        tcs.SetResult(response);
                    }
                    catch (Exception e)
                    {
                        //FIXME: Try deserialize an error message to put in an exception?
                        tcs.SetException(e);
                    }
                }

                PendingCalls.Add(correlationId, CallService);
            }

            await using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, request);

            _rabbitClient.Model.BasicPublish(
                exchange: _exchangeName,
                routingKey: _queueName,
                basicProperties: props,
                body: memoryStream.GetBuffer().AsMemory(..(int)memoryStream.Length));

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