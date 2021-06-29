using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol.Shared;
using Slate.Networking.RabbitMQ;
using Slate.Overseer.Configuration;

namespace Slate.Overseer
{
    internal class CoreApplicationStarter : IHostedService
    {
        private readonly IApplicationLauncher _applicationLauncher;
        private readonly ComponentSection _componentSection;
        private readonly IRabbitClient _rabbitClient;
        private readonly ILogger _logger;

        public CoreApplicationStarter(IApplicationLauncher applicationLauncher, ILogger logger, ComponentSection componentSection, IRabbitClient rabbitClient)
        {
            _applicationLauncher = applicationLauncher;
            _componentSection = componentSection;
            _rabbitClient = rabbitClient;
            _logger = logger.ForContext<CoreApplicationStarter>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Component Root Path: {_componentSection.ComponentRootPath}");
            foreach (var definition in _componentSection.Definitions.Where(d => d.LaunchOnStart))
            {
                _applicationLauncher.LaunchAsync(definition.Name);
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _rabbitClient.Send(new FullSystemShutdownMessage() { Reason = "Overseer application closed" });
            await _applicationLauncher.ExitAllApplicationsAsync();
            
        }


    }
}