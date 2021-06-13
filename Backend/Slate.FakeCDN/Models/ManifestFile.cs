#nullable disable // JSON + nullable sucks...
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Slate.FakeCDN.Models
{
    public class ManifestFile
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }
        [JsonPropertyName("md5")]
        public string MD5 { get; set; }
        [JsonPropertyName("sources")]
        public List<FileSource> Sources { get; set; }

        public bool Verify()
        {
            if (Path.Contains(".."))
            {
                Console.WriteLine("illegal sequence in manifest file path: '..' in {0}", Path);
                return false;
            }

            if (Path.Contains("~"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '~' in {0}", Path);
                return false;
            }

            if (Path.Contains("$"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '$' in {0}", Path);
                return false;
            }

            if (Path.Contains("%"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '%' in {0}", Path);
                return false;
            }

            if (System.IO.Path.IsPathFullyQualified(Path))
            {
                Console.WriteLine("file path is fully qualified: {0}", Path);
                return false;
            }

            if (Path.Contains("servers.json"))
            {
                Console.WriteLine("illegal sequence in manifest file path: 'servers.json' in {0}", Path);
                return false;
            }

            if (Path.Contains("Sunrise.exe"))
            {
                Console.WriteLine("illegal sequence in manifest file path: 'Sunrise.exe' in {0}", Path);
                return false;
            }

            return Sources.All(x => x.Verify());
        }
    }
}