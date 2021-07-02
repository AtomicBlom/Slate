using System.Collections.Generic;
using ProtoBuf;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract(SkipConstructor = true)]
    public class CharacterUpdateBatch : MessageToGameWarden
    {
        [ProtoMember(1)] public List<CharacterMovement> CharacterMovement { get; init; } = new ();

        public CharacterUpdateBatch(Uuid instanceId) : base(instanceId)
        {
        }
    }
}