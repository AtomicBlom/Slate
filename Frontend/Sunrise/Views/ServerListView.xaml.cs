using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SunriseLauncher.Views
{
    public class ServerListView : UserControl
    {
        public ServerListView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
