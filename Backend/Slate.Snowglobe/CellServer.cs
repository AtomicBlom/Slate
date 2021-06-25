using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.Snowglobe
{
    public class CellServer : IHostedService
    {
        private readonly ILogger<CellServer> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRabbitClient _rabbitClient;
        private readonly IConfiguration _configuration;

        public CellServer(ILogger<CellServer> logger, IHostApplicationLifetime lifetime, IRabbitClient rabbitClient, IConfiguration configuration)
        {
            _logger = logger;
            _lifetime = lifetime;
            _rabbitClient = rabbitClient;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");
            _rabbitClient.Send(new NotifyCellServerAwake()
            {
                Endpoint = _configuration["Id"]
            });

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
