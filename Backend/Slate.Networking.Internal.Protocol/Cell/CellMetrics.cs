using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract]
    public partial class CellMetrics
    {
        [ProtoMember(1)]
        public Uuid InstanceId { get; set; }

        [ProtoMember(2)]
        public uint PlayerCount { get; set; }

    }
}
