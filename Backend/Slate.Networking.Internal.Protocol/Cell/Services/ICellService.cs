using System.Collections.Generic;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Slate.Networking.Internal.Protocol.Cell.Services
{
    [Service]
    public interface ICellService
    {
        IAsyncEnumerable<CharacterUpdateBatch> SubscribeAsync(IAsyncEnumerable<UpdateToGameWarden> values, CallContext context = default);
    }
}