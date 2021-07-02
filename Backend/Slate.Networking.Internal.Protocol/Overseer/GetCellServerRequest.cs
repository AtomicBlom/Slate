using ProtoBuf;

namespace Slate.Networking.Internal.Protocol.Overseer
{
    [ProtoContract]
    public class GetCellServerRequest
    {
        [ProtoMember(1)]
        public string CellName { get; set; } = "";

    }
}