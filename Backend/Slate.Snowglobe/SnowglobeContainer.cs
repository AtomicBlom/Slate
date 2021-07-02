using System;
using MessagePipe;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _services;

        public SnowglobeContainer(IServiceProvider services) : base(services)
        {
            _services = services;
        }

        [Instance]
        private IServerAddressesFeature Addresses =>
            _services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()
            ?? throw new Exception($"Unable to resolve {nameof(IServerAddressesFeature)}");

        [Factory(Scope.InstancePerDependency)]
        IBufferedPublisher<T> ResolveBufferedPublisherOfT<T>() => _services.GetRequiredService<IBufferedPublisher<T>>();

        [Factory(Scope.InstancePerDependency)]
        IBufferedAsyncSubscriber<T> ResolveBufferedAsyncSubscriberOfT<T>() => _services.GetRequiredService<IBufferedAsyncSubscriber<T>>();
    }
}
