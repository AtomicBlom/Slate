using System;
using System.Threading.Tasks;

namespace Slate.Client.ViewModel.Services
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string? ErrorMessage)> DiscoverAuthServer();
        Task<string?> Login(string username, string password);
        event Action? LoggedIn;
    }
}
