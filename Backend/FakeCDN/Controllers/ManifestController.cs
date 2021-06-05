using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SunriseLauncher.Models;

namespace FakeCDN.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManifestController : ControllerBase
    {
        private readonly IWebHostBuilder _environment;
        private Task<byte[]>? _generateManifestTask;
        private CancellationTokenSource _currentTaskCancellation = new();
        private readonly object _lockObj = new();
        private readonly FileSystemWatcher _fsw;
        private string _contentDir;

        public ManifestController(IConfiguration configuration, IWebHostBuilder environment)
        {
            _environment = environment;
            _contentDir = Path.GetFullPath(configuration["FAKECDN_CONTENTDIR"]);
            _fsw = new FileSystemWatcher(_contentDir);
            _fsw.IncludeSubdirectories = true;
            _fsw.Created += (sender, args) => ResetManifest();
            _fsw.Changed += (sender, args) => ResetManifest();
            _fsw.Deleted += (sender, args) => ResetManifest();
            _fsw.Renamed += (sender, args) => ResetManifest();
            _fsw.EnableRaisingEvents = true;
        }

        private void ResetManifest()
        {
            lock (_lockObj)
            {
                _currentTaskCancellation.Cancel();
                _currentTaskCancellation = new CancellationTokenSource();
                _generateManifestTask = GenerateManifestFile();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            lock (_lockObj)
            {
                if (_generateManifestTask is null)
                {
                    ResetManifest();
                }
            }

            var manifestFile = await _generateManifestTask;
            return File(
                manifestFile,
                "text/xml",
                "Manifest.xml"
            );
        }

        private Task<byte[]> GenerateManifestFile()
        {
            var manifest = new Manifest()
            {
                Version = "1.0",
                LaunchOptions = new List<LaunchOption>()
                {
                    new()
                    {
                        Args = "--AccessToken {AccessToken} --Host localhost",
                        LaunchPath = "Client.exe",
                        Name = "Local development"
                    }
                },
                Files = new List<ManifestFile>()
            };
            var setting = _environment.GetSetting(WebHostDefaults.ServerUrlsKey);

            foreach (var file in Directory.GetFiles(_contentDir, "*.*", SearchOption.AllDirectories).Select(f => new FileInfo(f)))
            {
                if (_currentTaskCancellation.IsCancellationRequested) throw new TaskCanceledException();
                using var md5 = MD5.Create();
                using var stream = file.OpenRead();

                var hash = md5.ComputeHash(stream);
                var md5Result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                
                manifest.Files.Add(new ManifestFile
                {
                    MD5 = md5Result,
                    Path = file.FullName.Replace(_contentDir, ""),
                    Size = file.Length,
                });
            }


            throw new System.NotImplementedException();
        }
    }
}
