using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Events.InMemory;

namespace Slate.GameWarden
{
    public class MonitorEventBus : IHostedService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private IDisposable _subscription;

        public MonitorEventBus(IEventAggregator eventAggregator, ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _eventAggregator.GetEvent<object>()
                .Subscribe(new AnonymousObserver<object>(OnMessageReceived));

            return Task.CompletedTask;
        }

        public void OnMessageReceived(object message)
        {
            _logger
                .ForContext("Message", message, true)
                .Information("received a {MessageType}", message.GetType().FullName);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription.Dispose();

            return Task.CompletedTask;
        }
    }
}