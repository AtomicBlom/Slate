using System;
using Slate.Backend.Shared;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
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
        IContainer<HeartbeatService>
    {
        public GameContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}