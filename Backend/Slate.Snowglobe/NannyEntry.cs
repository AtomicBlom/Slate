using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slate.Snowglobe;

Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
        configHost
            .AddJsonFile("NannySettings.json")
            .AddEnvironmentVariables("NANNY_")
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
