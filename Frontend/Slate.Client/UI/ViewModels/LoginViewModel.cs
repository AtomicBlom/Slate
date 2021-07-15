using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using BinaryVibrance.INPCSourceGenerator;
using IdentityModel.Client;
using Slate.Client.UI.MVVM;

namespace Slate.Client.UI.ViewModels
{
    public partial class LoginViewModel
    {
        private readonly Uri _authServer;
        private readonly HttpClient _client;
        private DiscoveryDocumentResponse? _disco;
        private string _username = string.Empty;
        private string _password = string.Empty;

        [ImplementNotifyPropertyChanged(PropertyAccess.SetterPrivate)]
        private string _errorMessage = string.Empty;
        [ImplementNotifyPropertyChanged(ExposedType = typeof(ICommand))]
        private readonly RelayCommand _loginCommand;

        public event EventHandler<TokenResponse>? LoggedIn;

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

        public LoginViewModel(Uri authServer)
        {
            _authServer = authServer;
            _loginCommand = new RelayCommand(OnLogin, CanLogin);
            _client = new HttpClient();
        }

        private bool CanLogin() => true; /*_disco != null && 
                                             !string.IsNullOrWhiteSpace(Username) && 
                                             !string.IsNullOrWhiteSpace(Password);*/

        private async void OnLogin()
        {
            if (_disco == null) return;

            try
            {

                var result = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest()
                {
                    Address = _disco.TokenEndpoint,

                    ClientId = "Launcher",
                    ClientSecret = "secret",
                    Scope = "account offline_access",

                    UserName = Username,
                    Password = Password,
                });

                LoggedIn?.Invoke(this, result);
                
                Console.WriteLine(result.Raw);
            }
            catch (Exception e)
            {
                ErrorMessage = $"Failed to log in...\n{e.Message}";
            }
        }

        public async Task OnNavigatedTo()
        {
            _disco = await DiscoverAuthServer();
            RaisePropertyChanged(nameof(LoginCommand));
        }

        private async Task<DiscoveryDocumentResponse?> DiscoverAuthServer()
        {
            Console.WriteLine($"Getting Auth Server discovery document from {_authServer}...");
            try
            {
                var disco = await _client.GetDiscoveryDocumentAsync(_authServer.ToString());
                if (disco.IsError)
                {
                    Console.WriteLine($"Error retrieving discovery document");
                    Console.WriteLine(disco.Error);
                    ErrorMessage = disco.Error;
                    return null;
                }

                Console.WriteLine($"discovery document Successfully retrieved");
                
                return disco;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving discovery document");
                Console.WriteLine(e);
            }

            return null;
        }
    }
}