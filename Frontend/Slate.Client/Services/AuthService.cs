using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Serilog;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.Services
{
    public class AuthService : IAuthService, IProvideAuthToken
    {
        private readonly ILogger _logger;
        private readonly IUserLogEnricher _userLogEnricher;
        private DiscoveryDocumentResponse? _disco;
        private readonly Uri _authServer;

        public event Action? LoggedIn;
        private readonly HttpClient _client;

        public AuthService(Options options, ILogger logger, IUserLogEnricher userLogEnricher)
        {
            _logger = logger;
            _userLogEnricher = userLogEnricher;
            _authServer = options.AuthServer;
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
                    Password = password
                });

                var jwtHandler = new JwtSecurityTokenHandler();
                var jwt = jwtHandler.ReadJwtToken(result.AccessToken);
                
                _userLogEnricher.UserId = jwt.Subject;
                
                LoggedIn?.Invoke();

                _logger.Information("Successfully Logged in as {Username}", username);

                return null;
            }
            catch (Exception e)
            {
                return $"Failed to log in...\n{e.Message}";
            }
        }

        public string? AuthToken { get; }

        public async Task<(bool Succeeded, string? ErrorMessage)> DiscoverAuthServer()
        {
            _logger.Information("Getting Auth Server discovery document from {AuthServer}", _authServer);
            try
            {
                var disco = await _client.GetDiscoveryDocumentAsync(_authServer.ToString());
                if (disco.IsError)
                {
                    throw new Exception(disco.Error);
                }

                _disco = disco;
                _logger.Information("discovery document Successfully retrieved");
                return (true, null);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error retreiving discovery document");
                return (false, e.Message);
            }
        }
    }

    public interface IProvideAuthToken
    {
        string? AuthToken { get; }
    }
}
