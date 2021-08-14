namespace Slate.Client.ViewModel.MainMenu
{
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
}