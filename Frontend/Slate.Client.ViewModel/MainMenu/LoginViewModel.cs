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
        
        private string _username = string.Empty;
        private string _password = string.Empty;

        [ImplementNotifyPropertyChanged(PropertyAccess.SetterPrivate)]
        private string? _errorMessage = string.Empty;
        [ImplementNotifyPropertyChanged(ExposedType = typeof(ICommand))]
        private readonly RelayCommand _loginCommand;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            _loginCommand = new RelayCommand(OnLogin, CanLogin);
        }

        private async void OnLogin()
        {
            try
            {
                var failureMessage = await _authService.Login(Username, Password);
                ErrorMessage = failureMessage;
            }
            catch (Exception e)
            {
                //FIXME: Log this
            }
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

        private bool CanLogin() => true; /*_disco != null && 
                                             !string.IsNullOrWhiteSpace(Username) && 
                                             !string.IsNullOrWhiteSpace(Password);*/

        

        public async Task OnNavigatedTo()
        {
            await _authService.DiscoverAuthServer();
            RaisePropertyChanged(nameof(LoginCommand));
        }
    }
}