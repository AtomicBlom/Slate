using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Client.Annotations;

namespace Client.UI.ViewModels
{
    public class GameUIViewModel : INotifyPropertyChanged
    {
        private LoginViewModel? _loginViewModel;

        public LoginViewModel? LoginViewModel
        {
            get => _loginViewModel;
            set => SetField(ref _loginViewModel, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void RaisePropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [NotifyPropertyChangedInvocator]
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

    }
}
