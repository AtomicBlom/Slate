using System.Threading.Tasks;
using Game.CoreNetworking;
using ProtoBuf.Grpc;

namespace GameWarden.Services
{
    public class Auth : IAuth
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
