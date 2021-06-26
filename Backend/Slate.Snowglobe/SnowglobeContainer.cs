using System;
using Slate.Backend.Shared;
using Slate.Networking.Internal.Protocol.Cell.Services;
using StrongInject;

namespace Slate.Snowglobe
{
    [Register(typeof(CellServerNotifierService))]
    [Register(typeof(CellService), Scope.SingleInstance, typeof(ICellService))]
    internal partial class SnowglobeContainer : CoreServicesModule,
        IContainer<CellServerNotifierService>,
        IContainer<ICellService>,
        IContainer<HeartbeatService>,
        IContainer<GracefulShutdownService>
    {
        public SnowglobeContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
