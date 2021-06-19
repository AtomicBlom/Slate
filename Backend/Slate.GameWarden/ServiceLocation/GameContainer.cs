using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using StrongInject;

namespace Slate.GameWarden.ServiceLocation
{
    [Register(typeof(PlayerLocator), Scope.SingleInstance, typeof(IPlayerLocator))]
    [Register(typeof(CellPlayerService), typeof(IPlayerService))]
    [Register(typeof(CharacterCoordinator))]
    [RegisterModule(typeof(RabbitMQModule))]
    [RegisterModule(typeof(GrpcServicesModule))]
    internal partial class GameContainer : IContainer<IAccountService>, IContainer<IGameService>, IContainer<IAuthorizationService>, IContainer<Func<Guid, CharacterCoordinator>>
    {
        public GameContainer(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        [Instance] private readonly IConfiguration _configuration;

    }
}