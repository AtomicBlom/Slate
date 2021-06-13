using System;

namespace Slate.Networking.Internal.Protocol
{
    public partial class Uuid
    {
        public Guid ToGuid()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            BitConverter.TryWriteBytes(guidBytes[..8], (ulong)A);
            BitConverter.TryWriteBytes(guidBytes[8..16], (ulong)B);

            return new Guid(guidBytes);
        }
    }

    public static class GuidExtensions
    {
        public static Uuid ToUuid(this Guid guid)
        {
            Span<byte> guidBytes = guid.ToByteArray();
            var a = BitConverter.ToUInt64(guidBytes[..8]);
            var b = BitConverter.ToUInt64(guidBytes[8..16]);
            return new Uuid { A = a, B = b };
        }
    }
}
