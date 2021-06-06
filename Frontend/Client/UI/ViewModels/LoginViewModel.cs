using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Client.Annotations;
using EmptyKeys.UserInterface.Input;
using IdentityModel.Client;

namespace Client.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly Uri _authServer;
        private readonly HttpClient _client;
        private DiscoveryDocumentResponse? _disco;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private ICommand _loginCommand;

        public event EventHandler<TokenResponse>? LoggedIn;

        public ICommand LoginCommand
        {
            get => _loginCommand;
            set => SetField(ref _loginCommand, value);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetField(ref _username, value))
                {
                    RaisePropertyChanged(nameof(LoginCommand));
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
                    RaisePropertyChanged(nameof(LoginCommand));
                };
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        public LoginViewModel(Uri authServer)
        {
            _authServer = authServer;
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            _client = new HttpClient();
        }

        private bool CanLogin(object obj) => _disco != null && 
                                             !string.IsNullOrWhiteSpace(Username) && 
                                             !string.IsNullOrWhiteSpace(Password);

        private async void OnLogin(object obj)
        {
            if (_disco == null) return;

            try
            {

                var result = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest()
                {
                    Address = _disco.TokenEndpoint,

                    ClientId = "Launcher",
                    ClientSecret = "secret",
                    Scope = "account",

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string? propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [NotifyPropertyChangedInvocator]
        protected bool SetField<T>(ref T field, T value, [CallerMemberName]
            string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}