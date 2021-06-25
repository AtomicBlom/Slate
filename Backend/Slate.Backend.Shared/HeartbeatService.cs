using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.Backend.Shared
{
    public class HeartbeatService : IHostedService
    {
        private readonly IRPCClient _rpcClient;
        private readonly ILogger _logger;
        private volatile bool _running;

        public HeartbeatService(IRPCClient rpcClient, ILogger logger)
        {
            _rpcClient = rpcClient;
            _logger = logger.ForContext<HeartbeatService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Starting HeartbeatService");
            _running = true;
            Task.Run(async () =>
            {
                while (_running)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _rpcClient.CallAsync<HeartbeatRequest, HeartbeatResponse>(new HeartbeatRequest());
                    }
                    catch (Exception e)
                    {
                        Environment.Exit(-1);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _running = false;
            return Task.CompletedTask;
        }
    }

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
            _logger.Warning("Application is shutting down");
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