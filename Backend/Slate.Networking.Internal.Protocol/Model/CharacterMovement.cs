using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Model
{
    [ProtoContract]
    public partial class CharacterMovement
    {
        [ProtoMember(1)]
        public Uuid Id { get; set; }

    }
}