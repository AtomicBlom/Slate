using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
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
        
        services.AddLogging(lb => lb.AddSerilog(dispose: true));

        services.AddTransientServiceUsingContainer<OverseerContainer, IHostedService, ApplicationLauncher>();

        //services.AddHostedService<ApplicationLauncher>();
        
        //services.Configure<RabbitSettings>(hostContext.Configuration.GetSection(nameof(RabbitSettings)));
        //services.AddSingleton<IRabbitSettings>(sp => sp.GetRequiredService<IOptions<RabbitSettings>>().Value);
        //services.AddSingleton<IRabbitClient, RabbitClient>();
    })
    .Build()
    .Run();

