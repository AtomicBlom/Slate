using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(10000, typeof(CharacterUpdateBatch))]
    public abstract class MessageToGameWarden
    {
        public Uuid InstanceId { get; }
        protected MessageToGameWarden(Uuid instanceId)
        {
            InstanceId = instanceId;
        }

    }
}