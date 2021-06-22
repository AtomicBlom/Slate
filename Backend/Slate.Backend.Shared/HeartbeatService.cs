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
}