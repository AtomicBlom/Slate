using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using ReactiveUI;

namespace SunriseLauncher.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase(Window window)
        {
            Window = window;
        }

        public Window Window { get; }
    }
}
