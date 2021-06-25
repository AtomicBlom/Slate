using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.Snowglobe
{
    public class CellServer : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IRabbitClient _rabbitClient;
        private readonly IConfiguration _configuration;

        public CellServer(ILogger logger, IRabbitClient rabbitClient, IConfiguration configuration)
        {
            _logger = logger.ForContext<CellServer>();
            _rabbitClient = rabbitClient;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");
            _rabbitClient.Send(new NotifyCellServerAwake()
            {
                Id = Guid.Parse(_configuration["Id"]).ToUuid(),
                Endpoint = new Endpoint()
                {

                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
