using System;
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
        public SnowglobeContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
