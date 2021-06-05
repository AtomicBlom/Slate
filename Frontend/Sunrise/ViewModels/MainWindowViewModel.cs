using Avalonia.Controls;
using ReactiveUI;
using SunriseLauncher.Services;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

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
