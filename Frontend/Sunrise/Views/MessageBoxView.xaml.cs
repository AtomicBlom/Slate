using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SunriseLauncher.Views
{
    public class MessageBoxView : Window
    {
        public MessageBoxView()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public MessageBoxView(string message, string subMessage, bool enableCancel)
        {
            Message = message;
            SubMessage = subMessage;
            EnableCancel = enableCancel;

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public string Message { get; }
        public string SubMessage { get; }
        public bool EnableCancel { get; }

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
