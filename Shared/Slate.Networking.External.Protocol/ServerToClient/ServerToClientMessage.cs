using ProtoBuf;

namespace Slate.Networking.External.Protocol.ServerToClient
{
    [ProtoContract]
    [ProtoInclude(700, typeof(CharacterAdded))]
    [ProtoInclude(701, typeof(CharacterMoved))]
    [ProtoInclude(702, typeof(CharacterRemoved))]
    public class ServerToClientMessage
    {
    }
}