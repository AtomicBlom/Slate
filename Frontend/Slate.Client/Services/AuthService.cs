using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.Services
{
    public class AuthService : IAuthService
    {
        private DiscoveryDocumentResponse? _disco;
        private readonly Uri _authServer;

        public event EventHandler<TokenResponse>? LoggedIn;
        private readonly HttpClient _client;

        public AuthService(Uri authServer)
        {
            _authServer = authServer;
            _client = new HttpClient();
        }

        public async Task<string?> Login(string username, string password)
        {
            if (_disco == null) return "Auth server has not yet been discovered";

            try
            {

                var result = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest()
                {
                    Address = _disco.TokenEndpoint,

                    ClientId = "Launcher",
                    ClientSecret = "secret",
                    Scope = "account offline_access",

                    UserName = username,
                    Password = password,
                });

                LoggedIn?.Invoke(this, result);

                Console.WriteLine(result.Raw);
                return null;
            }
            catch (Exception e)
            {
                return $"Failed to log in...\n{e.Message}";
            }
        }

        public async Task<string?> DiscoverAuthServer()
        {
            Console.WriteLine($"Getting Auth Server discovery document from {_authServer}...");
            try
            {
                var disco = await _client.GetDiscoveryDocumentAsync(_authServer.ToString());
                if (disco.IsError)
                {
                    Console.WriteLine($"Error retrieving discovery document");
                    Console.WriteLine(disco.Error);
                    return disco.Error;
                }

                _disco = disco;
                Console.WriteLine($"discovery document Successfully retrieved");

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving discovery document");
                Console.WriteLine(e);
                return e.Message;
            }
        }
    }
}
