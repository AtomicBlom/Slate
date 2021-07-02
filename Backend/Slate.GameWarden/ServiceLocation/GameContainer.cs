using System;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Slate.Backend.Shared;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.Services;
using StrongInject;

namespace Slate.GameWarden.ServiceLocation
{
    [Register(typeof(PlayerLocator), Scope.SingleInstance, typeof(IPlayerLocator))]
    [Register(typeof(PlayerCellService), typeof(IPlayerService))]
    [Register(typeof(PlayerConnection))]
    [Register(typeof(CellConnectionManager), Scope.SingleInstance, typeof(ICellConnectionManager))]
    [RegisterModule(typeof(GrpcServicesModule))]
    internal partial class GameContainer : CoreServicesModule, 
        IContainer<IAccountService>, 
        IContainer<IGameService>, 
        IContainer<IAuthorizationService>, 
        IContainer<Func<CharacterIdentifier, PlayerConnection>>, 
        IContainer<HeartbeatService>,
        IContainer<GracefulShutdownService>
    {
        private readonly IServiceProvider _serviceProvider;

        public GameContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Factory(Scope.SingleInstance)]
        EventFactory ResolveEventFactory() => _serviceProvider.GetRequiredService<EventFactory>();

        [Factory(Scope.SingleInstance)]
        IBufferedPublisher<T> ResolveBufferedPublisherOfT<T>() => _serviceProvider.GetRequiredService<IBufferedPublisher<T>>();

        [Factory(Scope.SingleInstance)]
        IBufferedAsyncPublisher<T> ResolveBufferedAsyncPublisherOfT<T>() => _serviceProvider.GetRequiredService<IBufferedAsyncPublisher<T>>();

        [Factory(Scope.SingleInstance)]
        IBufferedAsyncSubscriber<T> ResolveBufferedAsyncSubscriberOfT<T>() => _serviceProvider.GetRequiredService<IBufferedAsyncSubscriber<T>>();
    }
}