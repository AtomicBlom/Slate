using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Services
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
