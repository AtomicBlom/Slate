using ProtoBuf;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Overseer
{
    [ProtoContract]
    public partial class GetCellServerResponse
    {
        [ProtoMember(1)]
        public Uuid Id { get; set; }

        [ProtoMember(2)]
        public Endpoint Endpoint { get; set; }

    }
}