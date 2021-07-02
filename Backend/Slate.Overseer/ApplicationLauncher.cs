using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Slate.Networking.Internal.Protocol;
using Slate.Overseer.Configuration;

namespace Slate.Overseer
{
    internal class ApplicationLauncher : IApplicationLauncher
    {
        private readonly List<Process> _managedProcesses = new();

        private readonly ILogger _logger;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ComponentSection _componentSection;
        private bool _running = true;

        public ApplicationLauncher(ILogger logger, IHostEnvironment hostingEnvironment, ComponentSection componentSection)
        {
            _logger = logger.ForContext<ApplicationLauncher>();
            _hostingEnvironment = hostingEnvironment;
            _componentSection = componentSection;
        }

        private void RelaunchApplication(string definition, Dictionary<string, string?> dictionary)
        {
            Task.Run(async () => await LaunchAsync(definition, dictionary));
        }


        public async Task LaunchAsync(string applicationDefinitionName, Dictionary<string, string?>? arguments = null)
        {
            if (!_running) throw new TaskCanceledException("Application is shutting down");
            arguments ??= new();

            var definition = _componentSection.Definitions.SingleOrDefault(s => s.Name == applicationDefinitionName)
                ?? throw new ArgumentException("unknown application name", nameof(applicationDefinitionName));

            string useHeartbeat = Debugger.IsAttached ? " --UseHeartbeat True" : string.Empty;

            var allArguments = definition.AdditionalArguments.Concat(
                arguments.Select(a => a.Value is null ? a.Key : $"{a.Key}={a.Value}"))
                .ToList();

            var additionalArguments = allArguments.Any()
                ? " " + string.Join(' ', allArguments)
                : string.Empty;
            
            var fileName = Path.GetFullPath(Path.Combine(_componentSection.ComponentRootPath, definition.Application));
            var startInfo = new ProcessStartInfo(fileName)
            {
                WorkingDirectory = Path.GetDirectoryName(fileName),
                UseShellExecute = true,
                Arguments = $"--Environment={_hostingEnvironment.EnvironmentName}{useHeartbeat}{additionalArguments}"
            };

            var tcs = new TaskCompletionSource();

            var _ = Task.Run(async () =>
            {
                try
                {
                    _logger
                        .ForContext("Parameters", startInfo.Arguments)
                        .ForContext("WorkingDirectory", startInfo.WorkingDirectory)
                        .Information("Starting application {ApplicationName}", startInfo.FileName);

                    var process = Process.Start(startInfo);
                    if (process is null)
                    {
                        throw new Exception($"Unable to start process {fileName}");
                    }
                    _managedProcesses.Add(process);
                    tcs.SetResult();
                    await process.WaitForExitAsync();
                    if (definition.LaunchOnStart)
                    {
                        RelaunchApplication(applicationDefinitionName, arguments);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error in component {definition.Application}");
                }
            });

            await tcs.Task;
        }

        public async Task ExitAllApplicationsAsync()
        {
            _running = false;
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
