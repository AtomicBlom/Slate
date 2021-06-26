using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.External.Protocol.ClientToServer
{
    [ProtoContract]
    public class ConnectToRequest : ClientToServerMessage
    {
        [ProtoMember(1)]
        public Uuid CharacterId { get; set; }

    }
}