using Slate.Client.Services;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.ViewModel.Services;
using Slate.Events.InMemory;
using StrongInject;

namespace Slate.Client
{
    [Register(typeof(CharacterService), Scope.SingleInstance, typeof(ICharacterService))]
    [Register(typeof(CharacterListViewModel))]
    public partial class GameScopeContainer : 
        IContainer<GameConnection>, 
        IContainer<ICharacterService>, 
        IContainer<CharacterListViewModel>
    {
        private readonly Options _options;
        private readonly Container _parent;

        public GameScopeContainer(Options options, Container parent)
        {
            _options = options;
            _parent = parent;
        }

        [Factory(Scope.SingleInstance)]
        private GameConnection CreateGameConnection(IProvideAuthToken authTokenProvider)
        {
            return new GameConnection(_options.GameServer, _options.GameServerPort, authTokenProvider);
        }

        [Instance] private IProvideAuthToken AuthTokenProvider => _parent.Resolve<IProvideAuthToken>().Value;

        [Instance] private IEventAggregator EventAggregator => _parent.Resolve<IEventAggregator>().Value;

    }
}