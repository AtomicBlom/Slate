using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Slate.Networking.External.Protocol.Model;

namespace Slate.Networking.External.Protocol.Services
{
    [Service]
    public interface IAccountService
    {
        ValueTask<GetCharactersReply> GetCharactersAsync(GetCharactersRequest request);
    }

    [ProtoContract]
    public class GetCharactersRequest { }

    [ProtoContract]
    public class GetCharactersReply
    {
        [ProtoMember(1)]
        public List<Character> Characters { get; set; } = new();

    }
}
