using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunriseLauncher.Models;

namespace SunriseLauncher.Services
{
    public class ServerFile
    {
        const string path = "./servers.json";

        public ServerFileJson Load()
        {
            if (!File.Exists(path))
                return null;

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<ServerFileJson>(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("exception while reading serverfile: {0}", ex.Message);
            }
            return null;
        }

        public void Save(IEnumerable<Server> servers, string selected)
        {
            var file = new ServerFileJson();
            file.Servers = servers.ToList();
            file.Selected = selected;

            var json = JsonSerializer.Serialize(file);
            File.WriteAllText(path, json);
        }

        public class ServerFileJson
        {
            [JsonPropertyName("servers")]
            public List<Server> Servers { get; set; }
            [JsonPropertyName("selected")]
            public string Selected { get; set; }
        }
    }
}
