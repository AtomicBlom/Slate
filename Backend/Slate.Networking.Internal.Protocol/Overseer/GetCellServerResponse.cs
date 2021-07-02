using System;
using ProtoBuf;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Shared.Protocol;

namespace Slate.Networking.Internal.Protocol.Overseer
{
    [ProtoContract(SkipConstructor = true)]
    public class GetCellServerResponse
    {
        public GetCellServerResponse(Guid id, Endpoint endpoint)
        {
            Id = id.ToUuid();
            Endpoint = endpoint;
        }

        [ProtoMember(1)]
        public Uuid Id { get; }

        [ProtoMember(2)]
        public Endpoint Endpoint { get; }

    }
}