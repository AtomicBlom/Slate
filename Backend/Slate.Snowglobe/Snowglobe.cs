using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slate.Backend.Shared;
using Slate.Snowglobe;

if (args.Any(a => a.Contains("--AttachDebugger")))
{
    if (!Debugger.IsAttached)
    {
        Debugger.Launch();
    }
    else
    {
        Debugger.Break();
    }
}

Console.Title = "Snowglobe (Cell Server)";

Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
        configHost
            .AddJsonFile("AppSettings.json")
            .AddEnvironmentVariables("SNOWGLOBE_")
            .AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddCoreSlateServices<SnowglobeContainer>(hostContext.Configuration);
        services.AddHostedServiceUsingContainer<SnowglobeContainer, CellServer>();
    })
    .Build()
    .Run();
