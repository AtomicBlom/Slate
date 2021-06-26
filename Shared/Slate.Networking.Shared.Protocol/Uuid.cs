using System;
using ProtoBuf;

namespace Slate.Networking.Shared.Protocol
{
    [ProtoContract]
    public class Uuid
    {
        [ProtoMember(1, Name = @"a", DataFormat = DataFormat.FixedSize)]
        public ulong A { get; set; }

        [ProtoMember(2, Name = @"b", DataFormat = DataFormat.FixedSize)]
        public ulong B { get; set; }

        public Guid ToGuid()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            BitConverter.TryWriteBytes(guidBytes[..8], (ulong)A);
            BitConverter.TryWriteBytes(guidBytes[8..16], (ulong)B);

            return new Guid(guidBytes);
        }
    }
}