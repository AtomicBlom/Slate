using Avalonia.Controls;

namespace SunriseLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Window window) : base(window)
        {
            ServerList = new ServerListViewModel(window);
        }

        public ServerListViewModel ServerList { get; set; }
    }
}
