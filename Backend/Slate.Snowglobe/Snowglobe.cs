using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slate.Snowglobe;

Console.Title = "Snowglobe (Cell Server)";
if (args.Any(a => a.Contains("--attachDebugger"))) Debugger.Break();


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
        services
            .AddLogging()
            .AddHostedService<CellServer>();
    })
    .Build()
    .Run();
