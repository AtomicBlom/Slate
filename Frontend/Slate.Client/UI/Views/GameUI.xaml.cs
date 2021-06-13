using EmptyKeys.UserInterface.Controls;
// ReSharper disable ConvertToAutoProperty

namespace Client.UI.Views
{
    public partial class GameUI
    {
        public Grid CurrentScreen
        {
            get => _currentScreen;
            set => _currentScreen = value;
        }
    }
}
