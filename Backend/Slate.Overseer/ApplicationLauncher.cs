using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;
using Slate.Overseer.Configuration;

namespace Slate.Overseer
{
    internal class ApplicationLauncher : IHostedService
    {
        private readonly List<Process> _managedProcesses = new();

        private readonly ILogger _logger;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRabbitClient _rabbitClient;
        private readonly ComponentSection _componentSection;
        private bool _running = true;

        public ApplicationLauncher(ILogger logger, IHostEnvironment hostingEnvironment, IHostApplicationLifetime lifetime, IConfiguration configuration, IRabbitClient rabbitClient)
        {
            _logger = logger.ForContext<ApplicationLauncher>();
            _hostingEnvironment = hostingEnvironment;
            _lifetime = lifetime;
            _rabbitClient = rabbitClient;
            _componentSection = configuration.GetSection("Components").Get<ComponentSection>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(OnStarted);

            return Task.CompletedTask;
        }
        
        private void OnStarted()
        {
            _logger.Information($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");
            _logger.Information($"Component Root Path: {_componentSection.ComponentRootPath}");

            foreach (var definition in _componentSection.Definitions.Where(d => d.LaunchOnStart))
            {
                LaunchComponent(definition);
            }
        }

        private void LaunchComponent(ComponentDefinition definition)
        {
            if (!_running) return;

            var fileName = Path.GetFullPath(Path.Combine(_componentSection.ComponentRootPath, definition.Application));
            var startInfo = new ProcessStartInfo(fileName)
            {
                WorkingDirectory = Path.GetDirectoryName(fileName),
                UseShellExecute = true,
                Arguments = $"--Environment={_hostingEnvironment.EnvironmentName}"
            };

            Task.Run(async () =>
            {
                try
                {
                    var process = Process.Start(startInfo);
                    if (process is null)
                    {
                        throw new Exception($"Unable to start process {fileName}");
                    }
                    _managedProcesses.Add(process);
                    await process.WaitForExitAsync();
                    OnProcessExited(definition);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error in component {definition.Application}");
                }
            });
        }

        private void OnProcessExited(ComponentDefinition definition)
        {
            if (definition.LaunchOnStart)
            {
                LaunchComponent(definition);
            }
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _running = false;
            _rabbitClient.Send(new FullSystemShutdownMessage());
            var exitTasks = _managedProcesses.Select(async mp =>
            {
                var delayTask = Task.Delay(TimeSpan.FromSeconds(30));
                var exitTask = mp.WaitForExitAsync();
                var finishedTask = await Task.WhenAny(delayTask, exitTask);
                if (finishedTask == delayTask)
                {
                    _logger.Warning("Process {ProcessName} did not exit after being sent the shutdown message", mp.ProcessName);
                    mp.Kill();
                }
            });

            await Task.WhenAll(exitTasks);
        }
    }
}
