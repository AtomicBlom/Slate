using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Cell.Services;

namespace Slate.Snowglobe
{
    public class CellService : ICellService
    {
        public IAsyncEnumerable<CharacterUpdateBatch> SubscribeAsync(IAsyncEnumerable<UpdateToGameWarden> values, CallContext context = default)
        {
            throw new NotImplementedException();
        }
    }
}
