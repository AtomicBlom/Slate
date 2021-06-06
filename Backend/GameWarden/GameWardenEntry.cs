using GameWarden;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
        configHost
            .AddJsonFile("GameWardenSettings.json")
            .AddEnvironmentVariables("GAMEWARDEN_")
            .AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddLogging()
            .AddHostedService<PlayerManager>();
    })
    .Build()
    .Run();