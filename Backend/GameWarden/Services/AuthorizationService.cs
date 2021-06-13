using System.Threading.Tasks;
using Game.Networking.External.Protocol;
using ProtoBuf.Grpc;

namespace GameWarden.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public ValueTask<AuthorizeReply> AuthorizeAsync(AuthorizeRequest value, CallContext context = default)
        {
            return ValueTask.FromResult(new AuthorizeReply
            {
                WasSuccessful = true,
            });
        }
    }
}
