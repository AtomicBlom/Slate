using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.External.Protocol.ServerToClient
{
    [ProtoContract]
    public class CharacterRemoved : ServerToClientMessage
    {
        [ProtoMember(1)]
        public Uuid Id { get; set; }

    }
}