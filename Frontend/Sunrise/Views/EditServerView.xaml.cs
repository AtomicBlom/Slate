using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SunriseLauncher.Views
{
    public class EditServerView : Window
    {
        public EditServerView()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CloseWithResult(string result)
        {
            Close(result);
        }
    }
}
