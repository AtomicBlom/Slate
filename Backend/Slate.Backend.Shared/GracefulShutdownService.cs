using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol.Shared;
using Slate.Networking.RabbitMQ;

namespace Slate.Backend.Shared
{
    public class GracefulShutdownService : IHostedService
    {
        private readonly IRabbitClient _rabbitClient;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private IDisposable _subscription;

        public GracefulShutdownService(IRabbitClient rabbitClient, IHostApplicationLifetime applicationLifetime, ILogger logger)
        {
            _rabbitClient = rabbitClient;
            _applicationLifetime = applicationLifetime;
            _logger = logger.ForContext<GracefulShutdownService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _rabbitClient.Subscribe<FullSystemShutdownMessage>(ReceiveShutdownRequest);
            return Task.CompletedTask;
        }

        public Task ReceiveShutdownRequest(FullSystemShutdownMessage message)
        {
            _logger.Warning("Application is shutting down because {ShutdownMessage}", message.Reason);
            _applicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            _subscription = null;
            return Task.CompletedTask;
        }
    }
}