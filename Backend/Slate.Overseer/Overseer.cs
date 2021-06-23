using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Server;
using Serilog;
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
            .Enrich.FromLogContext()
            .CreateLogger();
        
        services.AddLogging(lb => lb
            .ClearProviders()
            .AddSerilog(dispose: true));

        services.AddSingletonServiceUsingContainer<OverseerContainer, IHostedService, ApplicationLauncher>();

        if (Debugger.IsAttached)
        {
            services.AddTransientServiceUsingContainer<OverseerContainer, IHostedService, DebugHeartbeatService>();
        }
        
    })
    .
    .Build()
    .Run();