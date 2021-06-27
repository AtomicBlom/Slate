using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.RabbitMQ;
using Slate.Networking.Shared.Protocol;
using Endpoint = Slate.Networking.Internal.Protocol.Model.Endpoint;

namespace Slate.Snowglobe
{
    public class CellServerNotifierService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IRabbitClient _rabbitClient;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IServerAddressesFeature _serverAddressesFeature;

        public CellServerNotifierService(ILogger logger, 
            IRabbitClient rabbitClient, 
            IConfiguration configuration,
            IHostApplicationLifetime hostApplicationLifetime,
            IServerAddressesFeature serverAddressesFeature)
        {
            _logger = logger.ForContext<CellServerNotifierService>();
            _rabbitClient = rabbitClient;
            _configuration = configuration;
            _hostApplicationLifetime = hostApplicationLifetime;
            _serverAddressesFeature = serverAddressesFeature;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(ApplicationStarted);
            
            return Task.CompletedTask;
        }

        private void ApplicationStarted()
        {
            _logger.Information($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");

            var address = _serverAddressesFeature.Addresses.FirstOrDefault() ?? throw new Exception("Unable to get assigned port");
            var boundAddress = BindingAddress.Parse(address);
            
            _rabbitClient.Send(new NotifyCellServerAwake
            {
                Id = Guid.Parse(_configuration["Id"]).ToUuid(),
                Endpoint = new Endpoint
                {
                    Hostname = boundAddress.Host,
                    Port = (uint)boundAddress.Port,
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
