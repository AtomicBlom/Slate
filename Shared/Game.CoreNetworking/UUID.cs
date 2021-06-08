using System;

namespace Game.CoreNetworking
{
    public partial class Uuid
    {
        public Guid ToGuid()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            BitConverter.TryWriteBytes(guidBytes[..8], A);
            BitConverter.TryWriteBytes(guidBytes[8..16], B);

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
