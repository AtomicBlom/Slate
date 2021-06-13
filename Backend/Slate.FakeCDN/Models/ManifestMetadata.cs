#nullable disable // JSON + nullable sucks...
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SunriseLauncher.Models
{
    public class ManifestMetadata
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("launch_options")]
        public List<LaunchOption> LaunchOptions { get; set; }

        public bool Verify()
        {
            if (LaunchOptions.Count == 0)
            {
                Console.WriteLine("need at least one launch config");
                return false;
            }

            return LaunchOptions.All(x => x.Verify());
        }
    }
}