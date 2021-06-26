using ProtoBuf;

namespace Slate.Networking.Internal.Protocol.Cell
{
    [ProtoContract]
    [ProtoInclude(10000, typeof(CharacterUpdateBatch))]
    public partial class UpdateToGameWarden
    {
    }
}