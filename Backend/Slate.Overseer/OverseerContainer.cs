using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Backend.Shared;
using Slate.Overseer.Configuration;
using StrongInject;

namespace Slate.Overseer
{
    [Register(typeof(ApplicationLauncher), Scope.SingleInstance, typeof(IApplicationLauncher))]
    [Register(typeof(CoreApplicationStarter))]
    [Register(typeof(CellLauncher))]
    [Register(typeof(DebugHeartbeatService))]
    [RegisterModule(typeof(RabbitMQModule))]
    internal partial class OverseerContainer : IContainer<CoreApplicationStarter>, IContainer<CellLauncher>, IContainer<DebugHeartbeatService>
    {
        [Instance] private readonly IServiceProvider _serviceProvider;
        [Instance] private readonly IConfiguration _configuration;

        public OverseerContainer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        [Instance] private ComponentSection CreateComponentSection =>
            _configuration
                .GetSection("Components")
                .Get<ComponentSection>();

        [Instance] private IHostEnvironment CreateHostEnvironment => 
            _serviceProvider
                .GetRequiredService<IHostEnvironment>();

        [Factory(Scope.InstancePerDependency)]
        private ILogger CreateLogger() => Log.Logger;

    }
}
