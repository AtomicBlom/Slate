using Slate.Client.Services;
using Slate.Client.ViewModel.Services;
using StrongInject;

namespace Slate.Client
{
    [Register(typeof(CharacterService), Scope.SingleInstance, typeof(ICharacterService))]
    public partial class GameScopeContainer : IContainer<GameConnection>, IContainer<ICharacterService>
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

    }
}