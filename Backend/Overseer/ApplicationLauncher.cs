using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Overseer.Configuration;
using RabbitMQ.Client;

namespace Overseer
{
    internal class ApplicationLauncher : IHostedService
    {
        private readonly List<Process> _managedProcesses = new();

        private readonly ILogger<ApplicationLauncher> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ComponentSection _componentSection;
        private bool _running = true;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger, IHostApplicationLifetime lifetime, IConfiguration configuration)
        {
            _logger = logger;
            _lifetime = lifetime;
            _componentSection = configuration.GetSection("Components").Get<ComponentSection>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(OnStarted);
            _lifetime.ApplicationStopping.Register(OnStopping);

            var factory = new ConnectionFactory() { HostName = "localhost", VirtualHost = "management" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare("q.test", autoDelete: false);
            
            for (int i = 0; i < 10000; ++i)
            {
                string message = $"Hello world {i}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
            }

            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _running = false;
            foreach (var process in _managedProcesses)
            {
                //FIXME: Don't bloody kill them... Tell them to exit gracefully...
                process.Kill();
            }
        }

        private void OnStarted()
        {
            _logger.LogInformation($"{Assembly.GetEntryAssembly()?.GetName().Name} Started");
            _logger.LogInformation($"Component Root Path: {_componentSection.ComponentRootPath}");

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
                UseShellExecute = true
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
                    _logger.LogError(e, $"Error in component {definition.Application}");
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
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
