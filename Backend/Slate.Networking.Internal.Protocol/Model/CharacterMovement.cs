using System;
using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Model
{
    [ProtoContract(SkipConstructor = true)]
    public class CharacterMovement
    {
        public CharacterMovement(Guid playerId)
        {
            Id = playerId.ToUuid();
        }

        [ProtoMember(1)]
        public Uuid Id { get; }

    }
}