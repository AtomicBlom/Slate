using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.External.Protocol.ServerToClient
{
    [ProtoContract]
    public class CharacterMoved : ServerToClientMessage
    {
        [ProtoMember(1)]
        public Uuid Id { get; set; }

        [ProtoMember(2)]
        public Vector3 Position { get; set; }

        [ProtoMember(3)]
        public Vector3 Velocity { get; set; }

    }
}