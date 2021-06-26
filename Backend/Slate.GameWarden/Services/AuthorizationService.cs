using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.Services;

namespace Slate.GameWarden.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public ValueTask<AuthorizeReply> AuthorizeAsync(AuthorizeRequest value)
        {
            return ValueTask.FromResult(new AuthorizeReply
            {
                WasSuccessful = true,
            });
        }
    }
}
