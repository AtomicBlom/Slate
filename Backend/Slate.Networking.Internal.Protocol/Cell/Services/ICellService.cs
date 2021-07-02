using System;
using System.Collections.Generic;
using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Cell.Services
{
    [Service]
    public interface ICellService
    {
        IAsyncEnumerable<MessageToGameWarden> SubscribeAsync(IAsyncEnumerable<MessageToSnowglobe> messages, CallContext context = default);
    }
}