using ProtoBuf;

namespace Slate.Networking.Internal.Protocol.Shared
{
    [ProtoContract]
    public class FullSystemShutdownMessage
    {
        [ProtoMember(1)]
        public string? Reason { get; set; }
    }
}