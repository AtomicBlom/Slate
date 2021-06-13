#nullable disable // JSON + nullable sucks...
using System;
using System.Text.Json.Serialization;

namespace Slate.FakeCDN.Models
{
    public class FileSource
    {
        [JsonPropertyName("url")]
        public string URL { get; set; }

        public bool Verify()
        {
            if (URL.ToLower().Contains("file:"))
            {
                Console.WriteLine("illegal sequence in manifest source url: 'file:' in source {0}", URL);
                return false;
            }

            return true;
        }
    }
}