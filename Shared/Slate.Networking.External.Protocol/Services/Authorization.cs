using System.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Grpc.Configuration;

namespace Slate.Networking.External.Protocol.Services
{
    [Service]
    public interface IAuthorizationService
    {
        ValueTask<AuthorizeReply> AuthorizeAsync(AuthorizeRequest request);
    }

    [ProtoContract]
    public class AuthorizeRequest
    {

    }

    [ProtoContract]
    public class AuthorizeReply
    {
        [ProtoMember(1)]
        public bool WasSuccessful { get; set; }

        [ProtoMember(2)]
        public string ErrorMessage { get; set; } = "";
    }
}
