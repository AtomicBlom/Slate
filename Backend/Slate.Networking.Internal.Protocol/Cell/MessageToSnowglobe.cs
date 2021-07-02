using System;
using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(20000, typeof(ConnectPlayerMessage))]
    public abstract class MessageToSnowglobe
    {
        protected MessageToSnowglobe(Uuid instanceId)
        {
            InstanceId = instanceId;
        }

        public Uuid? InstanceId { get; }
    }

    [ProtoContract(SkipConstructor = true)]
    public class ConnectPlayerMessage : MessageToSnowglobe
    {
        public Uuid CharacterId { get; }
        public Vector3 Position { get; }

        public ConnectPlayerMessage(Uuid instanceId, Uuid characterId, Vector3 position) : base(instanceId)
        {
            CharacterId = characterId;
            Position = position;
        }
    }
}