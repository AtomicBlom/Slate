#nullable disable // JSON + nullable sucks...
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace SunriseLauncher.Models
{
    public class LaunchOption
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("path")]
        public string LaunchPath { get; set; }
        [JsonPropertyName("env")]
        public string Env { get; set; }
        [JsonPropertyName("args")]
        public string Args { get; set; }

        public bool Verify()
        {
            if (Path.IsPathFullyQualified(LaunchPath))
            {
                Console.WriteLine("launch path is fully qualified");
                return false;
            }

            return true;
        }
    }
}