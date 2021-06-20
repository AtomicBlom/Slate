using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;
using Slate.Overseer;

Console.Title = "Overseer (Service Orchestration)";
if (args.Any(a => a.Contains("--attachDebugger"))) Debugger.Break();

Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost
            .AddJsonFile("AppSettings.json")
            .AddEnvironmentVariables("OVERSEER_")
            .AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger();
        
        services.AddLogging(lb => lb
            .ClearProviders()
            .AddSerilog(dispose: true));

        services.AddTransientServiceUsingContainer<OverseerContainer, IHostedService, ApplicationLauncher>();

        if (Debugger.IsAttached)
        {
            services.AddTransientServiceUsingContainer<OverseerContainer, IHostedService, DebugHeartbeatService>();
        }

    })
    .Build()
    .Run();