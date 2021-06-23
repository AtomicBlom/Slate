using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StrongInject;

namespace Slate.Backend.Shared
{
    [RegisterModule(typeof(RabbitMQModule))]
    [Register(typeof(HeartbeatService))]
    public abstract partial class CoreServicesModule
    {
        protected CoreServicesModule(IServiceProvider serviceProvider)
        {
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        [Instance] protected IConfiguration Configuration { get; }

        [Factory(Scope.InstancePerDependency)]
        protected static ILogger CreateLogger() => Log.Logger;
    }
}
