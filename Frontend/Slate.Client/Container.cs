using System.Collections.Generic;
using CastIron.Engine;
using CastIron.Engine.Debugging;
using CastIron.Engine.Graphics.Camera;
using CastIron.Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Ui;
using Serilog;
using Serilog.Core;
using Slate.Client.Services;
using Slate.Client.UI;
using Slate.Client.ViewModel.Services;
using StrongInject;

namespace Slate.Client
{
    [Register(typeof(AuthService), Scope.SingleInstance, typeof(IAuthService), typeof(IProvideAuthToken))]
    [Register(typeof(DebugInfoSink), Scope.SingleInstance, typeof(IDebugInfoSink), typeof(IGameComponent))]
    [Register(typeof(Camera), Scope.SingleInstance, typeof(ICamera), typeof(IGameComponent))]
    [Register(typeof(DebugMovementComponent), Scope.SingleInstance, typeof(IGameComponent))]
    [Register(typeof(Metrics), Scope.SingleInstance, typeof(IGameComponent))]
    [Register(typeof(TaskDispatcher), Scope.SingleInstance, typeof(IGameComponent))]
    [Register(typeof(GameLifecycle))]
    public partial class Container : 
        IContainer<GameScopeContainer>, 
        IContainer<IGameComponent[]>, 
        IContainer<GameLifecycle>,
        IContainer<IProvideAuthToken>,
        IContainer<ICamera>
    {
        private readonly Game _game;
        private readonly Options _options;
        private readonly UiSystem _uiSystem;
        private readonly ILogger _logger;
        private readonly IUserLogEnricher _userLogEnricher;

        public Container(Game game, Options options, UiSystem uiSystem, ILogger logger, IUserLogEnricher userLogEnricher)
        {
            _game = game;
            _options = options;
            _uiSystem = uiSystem;
            _logger = logger;
            _userLogEnricher = userLogEnricher;
            InputBindings = GameInputBindings.CreateInputBindings(_game);
        }

        [Instance] private Game Game => _game;
        [Instance] private UiSystem UI => _uiSystem;
        [Instance] private Options StartupOptions => _options;
        [Instance] private GraphicsDevice GraphicsDevice => _game.GraphicsDevice;
        [Instance] private ILogger Logger => _logger;
        [Instance] private IUserLogEnricher userLogEnricher => _userLogEnricher;

        [Instance(StrongInject.Options.AsImplementedInterfaces)]
        private InputBindingManager<GameInputState> InputBindings { get; }

        [Factory]
        private GameScopeContainer CreateGameScope() => new GameScopeContainer(_options, this);


    }
}
