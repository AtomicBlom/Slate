using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using Slate.Networking.RabbitMQ.StrongInject;
using StrongInject;

namespace Slate.GameWarden.ServiceLocation
{
    [Register(typeof(PlayerLocator), Scope.SingleInstance, typeof(IPlayerLocator))]
    [Register(typeof(CellPlayerService), typeof(IPlayerService))]
    [Register(typeof(CharacterCoordinator))]
    [RegisterModule(typeof(RabbitMQModule))]
    [RegisterModule(typeof(GrpcServicesModule))]
    [Register(typeof(HeartbeatService))]
    internal partial class GameContainer : IContainer<IAccountService>, IContainer<IGameService>, IContainer<IAuthorizationService>, IContainer<Func<Guid, CharacterCoordinator>>, IContainer<HeartbeatService>
    {
        public GameContainer(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        [Instance] private readonly IConfiguration _configuration;
        
        [Factory(Scope.InstancePerDependency)]
        private ILogger CreateLogger() => Log.Logger;
    }
}