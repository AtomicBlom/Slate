using System.Collections.Generic;
using ProtoBuf;
using Slate.Networking.Internal.Protocol.Model;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract]
    public partial class CharacterUpdateBatch : UpdateToGameWarden
    {
        [ProtoMember(1)]
        public List<CharacterMovement> CharacterMovement { get; set; }

    }
}