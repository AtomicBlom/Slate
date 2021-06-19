using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using Slate.Networking.RabbitMQ;
using StrongInject;

namespace Slate.GameWarden
{
    [Register(typeof(AuthorizationService), Scope.SingleInstance, typeof(IAuthorizationService))]
    [Register(typeof(AccountService), Scope.SingleInstance, typeof(IAccountService))]
    [Register(typeof(GameService), Scope.SingleInstance, typeof(IGameService))]
    [Register(typeof(PlayerLocator), Scope.SingleInstance, typeof(IPlayerLocator))]
    [Register(typeof(CellPlayerService), typeof(IPlayerService))]
    [Register(typeof(RabbitClient), Scope.SingleInstance, typeof(IRabbitClient))]
    [Register(typeof(CharacterCoordinator))]
    internal partial class GameContainer : IContainer<IAccountService>, IContainer<IGameService>, IContainer<IAuthorizationService>, IContainer<Func<Guid, CharacterCoordinator>>
    {
        private readonly IServiceProvider _serviceProvider;

        public GameContainer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _rabbitSettings = _serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetSection(RabbitSettings.SectionName)
                .Get<RabbitSettings>();
        }
        
        [Instance] private IRabbitSettings _rabbitSettings;

        [Factory]
        IRPCServer CreateRPCServer(IRabbitClient client) => client.CreateRPCServer();

        [Factory]
        IRPCClient CreateRPCClient(IRabbitClient client) => client.CreateRPCClient();
    }
}