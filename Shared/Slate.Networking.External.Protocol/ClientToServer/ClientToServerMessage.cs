using ProtoBuf;

namespace Slate.Networking.External.Protocol.ClientToServer
{
    [ProtoContract]
    [ProtoInclude(500, typeof(ClientRequestMove))]
    [ProtoInclude(501, typeof(ConnectToRequest))]
    public class ClientToServerMessage 
    {
    }
}