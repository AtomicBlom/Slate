using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunriseLauncher.Models;

namespace FakeCDN.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManifestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManifestController> _logger;
        private Task<byte[]>? _generateManifestTask;
        private CancellationTokenSource _currentTaskCancellation = new();
        private readonly object _lockObj = new();
        private readonly FileSystemWatcher _fsw;
        private string _contentDir;

        public ManifestController(IConfiguration configuration, ILogger<ManifestController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var applicationLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                      throw new Exception("Could not determine the running application location");
            //FIXME: Consider if GameFiles is absolute
            var fullPath = Path.GetFullPath(Path.Combine(applicationLocation, configuration["GameFiles"]));
            _contentDir = Path.GetFullPath(fullPath);
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
                _generateManifestTask = Task.Run(GenerateManifestFile);
            }
        }

        [Route("/Manifest/metadata")]
        [HttpGet]
        public ActionResult GetMetadata()
        {
            var manifest = new ManifestMetadata();
            ApplyMetadata(manifest);

            var json = JsonSerializer.Serialize(manifest, manifest.GetType(), new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            var manifestBytes = Encoding.UTF8.GetBytes(json);

            return File(
                manifestBytes,
                "application/json",
                "ManifestMetadata.json"
            );
        }

        private void ApplyMetadata(ManifestMetadata manifestFile)
        {
            manifestFile.Version = "1.0";
            manifestFile.LaunchOptions = new List<LaunchOption>
            {
                new()
                {
                    Args = "--AuthServer https://localhost:8001 --GameServer localhost --Port 4000",
                    LaunchPath = "Client.exe",
                    Name = "Local development"
                }
            };
        }

        /// <summary>
        /// Returns a full manifest file
        /// </summary>
        /// <returns></returns>
        [Route("/Manifest")]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            lock (_lockObj)
            {
                if (_generateManifestTask is null)
                {
                    ResetManifest();
                }
                Debug.Assert(_generateManifestTask is not null);
            }

            var manifestFile = await _generateManifestTask;
            return File(
                manifestFile,
                "application/json",
                "Manifest.json"
            );
        }

        private byte[] GenerateManifestFile()
        {
            var manifest = new Manifest
            {
                Files = new List<ManifestFile>()
            };
            ApplyMetadata(manifest);
            var setting = _configuration["Urls"].Split(";");

            foreach (var file in Directory.GetFiles(_contentDir, "*.*", SearchOption.AllDirectories).Select(f => new FileInfo(f)))
            {
                if (_currentTaskCancellation.IsCancellationRequested) throw new TaskCanceledException();
                using var md5 = MD5.Create();
                using var sha256 = SHA256.Create();
                using var stream = file.OpenRead();

                var hash = md5.ComputeHash(stream);
                var md5Result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                stream.Seek(0, SeekOrigin.Begin);

                var sha256Bytes = sha256.ComputeHash(stream);
                var sha256Result = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLowerInvariant();

                var filePath = file.FullName.Replace(_contentDir, "").TrimStart('\\');

                _logger.LogInformation($"Hashed file {filePath}, md5: {md5Result}");

                manifest.Files.Add(new ManifestFile
                {
                    MD5 = md5Result,
                    Sha256 = sha256Result,
                    Path = filePath,
                    Size = file.Length,
                    Sources = setting.Select(s => new FileSource()
                    {
                        URL =$"{s}/GameFiles/{filePath.Replace("\\", "/").TrimStart('/')}"
                    }).ToList()
                });
            }

            var json = JsonSerializer.Serialize(manifest, manifest.GetType(), new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
