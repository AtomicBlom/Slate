using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.External.Protocol.ClientToServer
{
    [ProtoContract]
    public class ClientRequestMove : ClientToServerMessage
    {
        [ProtoMember(1)]
        public Vector3 Location { get; set; }

        [ProtoMember(2)]
        public Vector3 Velocity { get; set; }

    }
}