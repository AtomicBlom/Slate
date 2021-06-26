using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.Services;
using StrongInject;

namespace Slate.GameWarden.ServiceLocation
{
    [Register(typeof(AuthorizationService), Scope.SingleInstance, typeof(IAuthorizationService))]
    [Register(typeof(AccountService), Scope.SingleInstance, typeof(IAccountService))]
    [Register(typeof(GameService), Scope.SingleInstance, typeof(IGameService))]
    internal class GrpcServicesModule
    {

    }
}