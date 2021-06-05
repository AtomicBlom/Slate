using SunriseLauncher.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SunriseLauncher.Services
{
    public class SunriseJson : IManifest
    {
        private static HttpClient client = new HttpClient();
        private string URL;
        private Manifest Manifest;

        public SunriseJson(string url)
        {
            URL = url;
        }

        private async Task FetchManifest()
        {
            if (Manifest != null)
                return;

            try
            {
                var response = await client.GetAsync(URL);
                if (response.IsSuccessStatusCode)
                {
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    {
                        Manifest = await JsonSerializer.DeserializeAsync<Manifest>(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while retrieving manifest: {0}", ex.Message);
            }
        }

        public async Task<ManifestMetadata> GetMetadataAsync()
        {
            try
            {
                await FetchManifest();
                return Manifest;
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
                await FetchManifest();
                if (Manifest == null)
                    return null;

                return Manifest.Files;
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while retrieving manifest: {0}", ex.Message);
            }
            return null;
        }
    }
}
