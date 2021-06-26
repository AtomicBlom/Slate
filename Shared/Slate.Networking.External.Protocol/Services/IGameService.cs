using System.Collections.Generic;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Slate.Networking.External.Protocol.ClientToServer;
using Slate.Networking.External.Protocol.ServerToClient;

namespace Slate.Networking.External.Protocol.Services
{
    [Service]
    public interface IGameService
    {
        IAsyncEnumerable<ServerToClientMessage> SubscribeAsync(IAsyncEnumerable<ClientToServerMessage> values, CallContext context = default);
    }
}
