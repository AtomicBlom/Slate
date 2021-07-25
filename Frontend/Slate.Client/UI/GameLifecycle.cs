using System;
using System.Threading.Tasks;
using CastIron.Engine;
using MLEM.Ui;
using Slate.Client.Services;
using Slate.Client.UI.Elements;
using Slate.Client.UI.MVVM;
using Slate.Client.UI.Views;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.ViewModel.Services;
using Stateless;

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

    public enum GameTrigger
    {
        StateMachineStarted,
        AssetsStartedLoading,
        AssetsFinishedLoading,
        DiscoDownloadSucceeded,
        PlayerLoggedIn,
        ConnectionToServerEstablished,
        CharacterSelected,
        ConnectionFailed,
        Reconnect
    }

    internal class GameLifecycle
    {
        private readonly UiSystem _uiSystem;
        private readonly IAuthService _authService;
        private readonly GameConnection _gameConnection;
        private readonly StateMachine<GameState, GameTrigger> _gameStateMachine = new(GameState.BeforeUI);

        private readonly StateMachine<GameState, GameTrigger>.TriggerWithParameters<string> _connectionErrorTrigger =
            new(GameTrigger.ConnectionFailed);

        public GameLifecycle(UiSystem uiSystem, IAuthService authService, GameConnection gameConnection)
        {
            _uiSystem = uiSystem;
            _authService = authService;
            _gameConnection = gameConnection;

            _gameStateMachine.OnTransitioned((s) => Console.WriteLine($"Lifecycle transitioned from {s.Source} to {s.Destination} because {s.Trigger}"));
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
                .Permit(GameTrigger.DiscoDownloadSucceeded, GameState.ReadyToLogin);
            _gameStateMachine.Configure(GameState.ReadyToLogin)
                .OnEntry(OnEnterGameStateReadyToLogin)
                .Permit(GameTrigger.PlayerLoggedIn, GameState.ConnectToServer)
                .OnExit(OnExitGameStateReadyToLogin);
            _gameStateMachine.Configure(GameState.ConnectToServer)
                .OnEntryAsync(OnEnterGameStateConnectToServer)
                .Permit(GameTrigger.ConnectionToServerEstablished, GameState.SelectingCharacter)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed);
            _gameStateMachine.Configure(GameState.SelectingCharacter)
                .OnEntryAsync(OnEnterGameStateSelectingCharacter)
                .Permit(GameTrigger.CharacterSelected, GameState.InGame)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed)
                .OnExit(OnExitGameStateSelectingCharacter);
            _gameStateMachine.Configure(GameState.ConnectionFailed)
                .OnEntryFrom(_connectionErrorTrigger, OnEnterGameStateConnectionFailed)
                .Permit(GameTrigger.Reconnect, GameState.ReadyToLogin);
            _gameStateMachine.Configure(GameState.InGame)
                .OnEntry(OnEnterGameStateInGame)
                .Permit(GameTrigger.ConnectionFailed, GameState.ConnectionFailed);
        }

        private void OnEnterGameStateIntroCards()
        {

            var viewModel = new IntroCardsViewModel
            {
                NextCommand = new RelayCommand(() => _gameStateMachine.Fire(GameTrigger.AssetsStartedLoading))
            };
            _uiSystem.Add(nameof(IntroCardsView), IntroCardsView.CreateView(viewModel));
        }
        private void OnExitGameStateIntroCards()
        {
            _uiSystem.Remove(nameof(IntroCardsView));
        }

        private void OnEnterGameStateLoading()
        {
            _gameStateMachine.Fire(GameTrigger.AssetsFinishedLoading);
        }

        private void OnEnterGameStateDownloadingDISCO()
        {
            _gameStateMachine.Fire(GameTrigger.DiscoDownloadSucceeded);
        }

        private void OnEnterGameStateReadyToLogin()
        {
            var loginViewModel = new LoginViewModel(_authService, () => _gameStateMachine.FireAsync(GameTrigger.PlayerLoggedIn))
            {
                Username = "atomicblom",
                Password = "password"
            };
            Task.Run(loginViewModel.OnNavigatedTo);
            _uiSystem.Add(nameof(LoginView), LoginView.CreateView(loginViewModel));
        }

        private void OnExitGameStateReadyToLogin()
        {
            TaskDispatcher.FireAndForget(async () => await 
                _uiSystem.Get(nameof(LoginView)).Element
                    .FadeOutAsync(remove:true)
            );
        }

        private async Task OnEnterGameStateConnectToServer()
        {
            var (wasSuccessful, errorMessage) = await _gameConnection.Connect();
            if (wasSuccessful)
            {
                await _gameStateMachine.FireAsync(GameTrigger.ConnectionToServerEstablished);
            }
            else
            {
                await _gameStateMachine.FireAsync(_connectionErrorTrigger, errorMessage ?? "Connection failed, but no error was reported");
            }
        }

        private async Task OnEnterGameStateSelectingCharacter()
        {
            var characterService = new CharacterService(_gameConnection);
            var characterListViewModel = new CharacterListViewModel(characterService, () => _gameStateMachine.FireAsync(GameTrigger.CharacterSelected));
            _uiSystem.Add(nameof(CharacterListView), CharacterListView.CreateView(characterListViewModel));
            await Task.Run(characterListViewModel.OnNavigatedTo);
        }

        private void OnExitGameStateSelectingCharacter()
        {
            _uiSystem.Get(nameof(CharacterListView)).Element
                .FadeOutAsync(remove: true);

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
