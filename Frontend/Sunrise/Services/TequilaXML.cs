using SunriseLauncher.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SunriseLauncher.Services
{
    public class TequilaXML : IManifest
    {
        private static HttpClient client = new HttpClient();

        private TequilaRoot TequilaRoot;

        private string Hash;

        private string URL;

        public TequilaXML(string url)
        {
            URL = url;
        }

        private async Task FetchManifest()
        {
            if (TequilaRoot != null)
                return;

            try
            {
                var serializer = new XmlSerializer(typeof(TequilaRoot));
                var response = await client.GetAsync(URL);
                if (response.IsSuccessStatusCode)
                {
                    var hash = SHA256.Create();
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    using (var hashstream = new CryptoStream(reader, hash, CryptoStreamMode.Read))
                    {
                        TequilaRoot = (TequilaRoot)serializer.Deserialize(hashstream);
                        Hash = Hashing.ByteArrayToHex(hash.Hash);
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
            await FetchManifest();
            if (TequilaRoot == null)
                return null;

            var metadata = new ManifestMetadata();
            metadata.Version = Hash;
            metadata.LaunchOptions = new List<LaunchOption>();

            foreach (var profile in TequilaRoot.Profiles)
            {
                var config = new LaunchOption();
                config.Name = profile.Value;
                config.LaunchPath = profile.Exec;
                config.Args = profile.Params;
                metadata.LaunchOptions.Add(config);
            }

            return metadata;
        }

        public async Task<IList<ManifestFile>> GetFilesAsync()
        {
            await FetchManifest();
            if (TequilaRoot == null)
                return null;

            var files = new List<ManifestFile>();
            foreach (var tequilaFile in TequilaRoot.FileList)
            {
                var file = new ManifestFile();
                file.MD5 = tequilaFile.MD5.ToLower();
                file.Path = tequilaFile.Name;
                file.Size = tequilaFile.Size;
                file.Sources = new List<FileSource>();
                foreach (var url in tequilaFile.URL)
                {
                    var source = new FileSource();
                    source.URL = url;
                    file.Sources.Add(source);
                }
                files.Add(file);
            }
            return files;
        }
    }

    [XmlRoot("manifest")]
    public class TequilaRoot
    {
        [XmlElement("label")]
        public string Label { get; set; }
        [XmlArray("profiles")]
        [XmlArrayItem("launch")]
        public List<TequilaProfile> Profiles { get; set; }
        [XmlArray("filelist")]
        [XmlArrayItem("file")]
        public List<TequilaFile> FileList { get; set; }
    }

    public class TequilaProfile
    {
        [XmlAttribute("params")]
        public string Params { get; set; }
        [XmlAttribute("exec")]
        public string Exec { get; set; }
        [XmlText]
        public string Value { get; set; }
    }

    public class TequilaFile
    {
        [XmlAttribute("md5")]
        public string MD5 { get; set; }
        [XmlAttribute("size")]
        public long Size { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("url")]
        public List<string> URL { get; set; }
    }
}
