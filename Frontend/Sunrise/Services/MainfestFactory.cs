using SunriseLauncher.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SunriseLauncher.Services
{
    public class MainfestFactory
    {
        const string sunrise_api = "sunrise-api";
        const string sunrise_json = "sunrise-json";
        const string tequila_xml = "tequila-xml";

        public static IManifest Get(string manifesturl)
        {
            var schema = getSchema(manifesturl);
            switch (schema)
            {
                case sunrise_api:
                    return new SunriseApi(manifesturl);
                case sunrise_json:
                    return new SunriseJson(manifesturl);
                case tequila_xml:
                    return new TequilaXML(manifesturl);
            }
            return null;
        }

        private static string getSchema(string manifesturl)
        {
            if (manifesturl.ToLower().EndsWith(".xml"))
                return tequila_xml;
            else if (manifesturl.ToLower().EndsWith(".json"))
                return sunrise_json;
            else
                return sunrise_api;
        }
    }

    public interface IManifest
    {
        public Task<ManifestMetadata> GetMetadataAsync();

        public Task<IList<ManifestFile>> GetFilesAsync();
    }
}
