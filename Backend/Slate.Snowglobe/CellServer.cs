using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Slate.Snowglobe
{
    public class CellServer : IHostedService
    {
        private readonly ILogger<CellServer> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public CellServer(ILogger<CellServer> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
