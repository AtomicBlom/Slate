using SunriseLauncher.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SunriseLauncher.Services
{
    public class SunriseApi : IManifest
    {
        private static HttpClient client = new HttpClient();
        private string URL;

        public SunriseApi(string url)
        {
            URL = url;
        }

        public async Task<ManifestMetadata> GetMetadataAsync()
        {
            try
            {
                var response = await client.GetAsync(URL + "/metadata");
                if (response.IsSuccessStatusCode)
                {
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    {
                        var manifest = await JsonSerializer.DeserializeAsync<ManifestMetadata>(reader);
                        return manifest;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while retrieving manifest: {0}", ex.Message);
            }
            return null;
        }

        public async Task<IList<ManifestFile>> GetFilesAsync()
        {
            try
            {
                var response = await client.GetAsync(URL);
                if (response.IsSuccessStatusCode)
                {
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    {
                        var manifest = await JsonSerializer.DeserializeAsync<Manifest>(reader);
                        return manifest.Files;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while retrieving manifest: {0}", ex.Message);
            }
            return null;
        }
    }
}
