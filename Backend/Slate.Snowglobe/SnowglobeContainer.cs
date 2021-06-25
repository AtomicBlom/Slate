using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Slate.Backend.Shared;
using StrongInject;

namespace Slate.Snowglobe
{
    [Register(typeof(CellServer))]
    internal partial class SnowglobeContainer : CoreServicesModule,
        IContainer<CellServer>,
        IContainer<HeartbeatService>,
        IContainer<GracefulShutdownService>
    {
        private readonly string _id;

        public SnowglobeContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            _id = config["Id"];
        }

        protected override ILogger CreateLogger()
        {
            return base.CreateLogger().ForContext("CellInstanceId", _id);
        }
    }
}
