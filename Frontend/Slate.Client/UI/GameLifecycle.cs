using System;
using System.Threading.Tasks;
using CastIron.Engine;
using MLEM.Ui;
using Myra.Graphics2D.UI;
using Serilog;
using Slate.Client.Services;
using Slate.Client.UI.Elements;
using Slate.Client.UI.MVVM;
using Slate.Client.UI.Views;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.ViewModel.Services;
using Slate.Events.InMemory;
using Stateless;
using StrongInject;

namespace Slate.Client.UI
{
    public enum GameState
    {
        BeforeUI,
        IntroCards,
        Loading,
        DownloadingDISCO,
        ReadyToLogin,
        ConnectionFailed,
        ConnectToServer,
        SelectingCharacter,
        InGame
    }

    internal class GameLifecycle
    {
        private readonly IUIManager _uiManager;
        private readonly Container _container;
        private readonly Func<GameScopeContainer> _gameScopeFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly StateMachine<GameState, GameTrigger> _gameStateMachine = new(GameState.BeforeUI);
        private readonly StateMachine<GameState, GameTrigger>.TriggerWithParameters<string> _connectionErrorTrigger =
            new(GameTrigger.ConnectionFailed);

        private GameScopeContainer _gameScopeContainer;
        private Owned<GameConnection> _gameConnection;
        
        public GameLifecycle(IUIManager uiManager, Container container, Func<GameScopeContainer> gameScopeFactory, IEventAggregator eventAggregator, ILogger logger)
        {
            _uiManager = uiManager;
            _container = container;
            _gameScopeFactory = gameScopeFactory;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _eventAggregator.GetEvent<GameTrigger>()
                .Subscribe((trigger) => _gameStateMachine.Fire(trigger));

            _gameStateMachine.OnTransitioned((s) => _logger.Information("Lifecycle transitioned from {SourceState} to {DestinationState} because {Trigger}", s.Source, s.Destination, s.Trigger));
            _gameStateMachine.Configure(GameState.BeforeUI)
                .Permit(GameTrigger.StateMachineStarted, GameState.IntroCards);
            _gameStateMachine.Configure(GameState.IntroCards)
                .OnEntry(OnEnterGameStateIntroCards)
                .Permit(GameTrigger.AssetsStartedLoading, GameState.Loading)
                .OnExit(OnExitGameStateIntroCards);
            _gameStateMachine.Configure(GameState.Loading)
                .OnEntry(OnEnterGameStateLoading)
                .Permit(GameTrigger.AssetsFinishedLoading, GameState.DownloadingDISCO);
            _gameStateMachine.Configure(GameState.DownloadingDISCO)
                .OnEntry(OnEnterGameStateDownloadingDISCO)
                .OnExit(OnExitGameStateDownloadingDISCO)
                .Permit(GameTrigger.DiscoDownloadSucceeded, GameState.ReadyToLogin);
            _gameStateMachine.Configure(GameState.ReadyToLogin)
                .OnEntry(OnEnterGameStateReadyToLogin)
                .Permit(GameTrigger.PlayerLoggedIn, GameState.ConnectToServer)
                .OnExit(OnExitGameStateReadyToLogin);
            _gameStateMachine.Configure(GameState.ConnectToServer)
                .OnEntry(OnEnterGameStateConnectToServer)
                .OnExit(OnExitGameStateConnectToServer)
                .Permit(GameTrigger.ConnectionToServerEstablished, GameState.SelectingCharacter)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed);
            _gameStateMachine.Configure(GameState.SelectingCharacter)
                .SubstateOf(GameState.ConnectToServer)
                .OnEntry(OnEnterGameStateSelectingCharacter)
                .Permit(GameTrigger.CharacterSelected, GameState.InGame)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed)
                .OnExit(OnExitGameStateSelectingCharacter);
            _gameStateMachine.Configure(GameState.ConnectionFailed)
                .OnEntryFrom(_connectionErrorTrigger, OnEnterGameStateConnectionFailed)
                .Permit(GameTrigger.Reconnect, GameState.ReadyToLogin);
            _gameStateMachine.Configure(GameState.InGame)
                .SubstateOf(GameState.ConnectToServer)
                .OnEntry(OnEnterGameStateInGame)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed);
        }

        private void OnEnterGameStateIntroCards()
        {
            _uiManager.ShowScreen(_container, new IntroCardsView());
        }

        private void OnExitGameStateIntroCards()
        {
            _uiManager.RemoveScreen<IntroCardsView>();
        }

        private void OnEnterGameStateLoading()
        {
            _gameStateMachine.Fire(GameTrigger.AssetsFinishedLoading);
        }

        private void OnEnterGameStateDownloadingDISCO()
        {
            _uiManager.ShowScreen(_container, new ContactingAuthServerView());
        }

        private void OnExitGameStateDownloadingDISCO()
        {
            _uiManager.FadeAndRemoveScreen<ContactingAuthServerView>();
        }

        private void OnEnterGameStateReadyToLogin()
        {
            _uiManager.ShowScreen(_container, new LoginView(), viewModel =>
            {
                viewModel.Username = "atomicblom";
                viewModel.Password = "password";
            });
        }

        private void OnExitGameStateReadyToLogin()
        {
            _uiManager.FadeAndRemoveScreen<LoginView>();
        }

        private void OnEnterGameStateConnectToServer()
        {
            TaskDispatcher.FireAndForget(async () => {
                _gameScopeContainer = _gameScopeFactory();
                _gameConnection = _gameScopeContainer.Resolve<GameConnection>();

                var (wasSuccessful, errorMessage) = await _gameConnection.Value.Connect();
                if (wasSuccessful)
                {
                    _gameStateMachine.Fire(GameTrigger.ConnectionToServerEstablished);
                }
                else
                {
                    _gameStateMachine.Fire(_connectionErrorTrigger, errorMessage ?? "Connection failed, but no error was reported");
                }
            });
        }

        private void OnExitGameStateConnectToServer()
        {
            //_gameConnection.Dispose();
            //_gameScopeContainer.Dispose();
        }

        private void OnEnterGameStateSelectingCharacter()
        {
            _uiManager.ShowScreen(_gameScopeContainer, new CharacterListView());
        }

        private void OnExitGameStateSelectingCharacter()
        {
            _uiManager.FadeAndRemoveScreen<CharacterListView>();
        }

        private void OnEnterGameStateConnectionFailed(string message)
        {
            throw new NotImplementedException();
        }

        private void OnEnterGameStateInGame()
        {
            
        }

        public void Start()
        {
            _gameStateMachine.Fire(GameTrigger.StateMachineStarted);

        }
    }
}
