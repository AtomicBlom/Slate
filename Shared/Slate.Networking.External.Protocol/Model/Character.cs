using ProtoBuf;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.External.Protocol.Model
{
    [ProtoContract]
    public class Character
    {
        [ProtoMember(1)]
        public Uuid Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }
}