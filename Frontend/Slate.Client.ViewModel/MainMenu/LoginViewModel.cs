using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.UI.MVVM;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.ViewModel.MainMenu
{
    public partial class LoginViewModel
    {
        private readonly IAuthService _authService;
        private readonly Action _loggedInAction;

        private string _username = string.Empty;
        private string _password = string.Empty;

        [ImplementNotifyPropertyChanged(PropertyAccess.SetterPrivate)]
        private string? _errorMessage = string.Empty;
        [ImplementNotifyPropertyChanged(ExposedType = typeof(ICommand))]
        private readonly RelayCommand _loginCommand;

        public LoginViewModel(IAuthService authService, Action loggedInAction)
        {
            _authService = authService;
            _loggedInAction = loggedInAction;
            _loginCommand = new RelayCommand(OnLogin, CanLogin);
        }

        private async void OnLogin()
        {
            try
            {
                _authService.LoggedIn += AuthServiceOnLoggedIn;
                var failureMessage = await _authService.Login(Username, Password);
                ErrorMessage = failureMessage;
            }
            catch (Exception e)
            {
                //FIXME: Log this
            }
            finally
            {
                _authService.LoggedIn -= AuthServiceOnLoggedIn;
            }
        }

        private void AuthServiceOnLoggedIn()
        {
            _loggedInAction?.Invoke();
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetField(ref _username, value))
                {
                    _loginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value))
                {
                    _loginCommand.RaiseCanExecuteChanged();
                };
            }
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
}