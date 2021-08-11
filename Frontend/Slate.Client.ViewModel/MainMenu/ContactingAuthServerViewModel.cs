using System;
using System.Threading.Tasks;
using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.ViewModel.MainMenu
{
    public partial class ContactingAuthServerViewModel
    {
        private readonly IAuthService _authService;
        private readonly Action _discoDiscoveredAction;
        private int _attempts = 0;

        [ImplementNotifyPropertyChanged(PropertyAccess.SetterPrivate)]
        private string _errorMessage = string.Empty;

        public ContactingAuthServerViewModel(IAuthService authService, Action discoDiscoveredAction)
        {
            _authService = authService;
            _discoDiscoveredAction = discoDiscoveredAction;
        }

        public async Task OnNavigatedTo()
        {
            bool discoDiscovered = false;
            while (!discoDiscovered)
            {
                _attempts++;
                var (succeeded, errorMessage) = await _authService.DiscoverAuthServer();
                discoDiscovered = succeeded;
                if (!succeeded)
                {
                    ErrorMessage = $"{errorMessage}\nAttempts: {_attempts}" ;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            _discoDiscoveredAction();
        }
    }
}