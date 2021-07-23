using System;
using System.Threading.Tasks;

namespace Slate.Client.ViewModel.Services
{
    public interface IAuthService
    {
        Task<string?> DiscoverAuthServer();
        Task<string?> Login(string username, string password);
        event Action? LoggedIn;
    }
}
