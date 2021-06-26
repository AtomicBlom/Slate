using System;

namespace Slate.Networking.Shared.Protocol
{
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
