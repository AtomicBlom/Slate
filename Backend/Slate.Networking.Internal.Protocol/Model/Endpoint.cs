using ProtoBuf;

namespace Slate.Networking.Internal.Protocol.Model
{
    [ProtoContract]
    public partial class Endpoint
    {
        [ProtoMember(1)]
        public string Hostname { get; set; } = "";

        [ProtoMember(2)]
        public uint Port { get; set; }

    }
}