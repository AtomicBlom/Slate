using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Overseer;

Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
        configHost
            .AddJsonFile("OverseerSettings.json")
            .AddEnvironmentVariables("OVERSEER_")
            .AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddLogging()
            .AddHostedService<ApplicationLauncher>();
    })
    .Build()
    .Run();

