using ProtoBuf;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract]
    public class NotifyCellServerAwake
    {
        [ProtoMember(1)] public Uuid Id { get; set; } = null!;

        [ProtoMember(2)] public Endpoint Endpoint { get; set; } = null!;

    }
}