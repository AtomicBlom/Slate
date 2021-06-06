using SunriseLauncher.Models;
using System;
using System.Security.Cryptography;

namespace SunriseLauncher.Services
{
    public class Hashing
    {
        public static HashAlgorithm GetHashAlgorithm(ManifestFile file)
        {
            if (!string.IsNullOrWhiteSpace(file.Sha256))
            {
                return SHA256.Create();
            }
            if (!string.IsNullOrWhiteSpace(file.MD5))
            {
                return MD5.Create();
            }
            Console.WriteLine("missing checksum for file '{0}'", file.Path);
            return null;
        }

        public static bool VerifyChecksum(byte[] bytes, ManifestFile file)
        {
            var checksum = ByteArrayToHex(bytes);
            if (!string.IsNullOrWhiteSpace(file.Sha256))
            {
                return checksum == file.Sha256;
            }
            if (!string.IsNullOrWhiteSpace(file.MD5))
            {
                return checksum == file.MD5;
            }
            return false;
        }

        public static string ByteArrayToHex(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x57 : b + 0x30);
                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x57 : b + 0x30);
            }
            return new string(c);
        }
    }
}
