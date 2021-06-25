using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StrongInject;

namespace Slate.Backend.Shared
{
    [RegisterModule(typeof(RabbitMQModule))]
    [Register(typeof(HeartbeatService))]
    [Register(typeof(GracefulShutdownService))]
    public abstract partial class CoreServicesModule
    {
        protected CoreServicesModule(IServiceProvider serviceProvider)
        {
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            HostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        }

        [Instance] protected IConfiguration Configuration { get; }
        [Instance] protected IHostApplicationLifetime HostApplicationLifetime { get; }

        [Factory(Scope.InstancePerDependency)]
        protected virtual ILogger CreateLogger() => Log.Logger;
    }
}
