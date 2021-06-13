using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Slate.Networking.RabbitMQ;
using Slate.Overseer;

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
        
        services.Configure<RabbitSettings>(hostContext.Configuration.GetSection(nameof(RabbitSettings)));
        services.AddSingleton<IRabbitSettings>(sp => sp.GetRequiredService<IOptions<RabbitSettings>>().Value);
        services.AddSingleton<IRabbitClient, RabbitClient>();
    })
    .Build()
    .Run();

