using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slate.Overseer
{
    public interface IApplicationLauncher
    {
        Task LaunchAsync(string applicationDefinitionName, Dictionary<string, string?>? arguments = null);
        Task ExitAllApplicationsAsync();
    }
}