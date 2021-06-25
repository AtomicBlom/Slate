using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StrongInject;

namespace Slate.Backend.Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreSlateServices<TContainer>(this IServiceCollection services, IConfiguration configuration) 
            where TContainer : CoreServicesModule, IContainer<HeartbeatService>, IContainer<GracefulShutdownService>
        {
            services.AddCoreSlateLogging(configuration);

            if (bool.TryParse(configuration["UseHeartbeat"], out var useHeartbeat) && useHeartbeat)
            {
                services.AddHostedServiceUsingContainer<TContainer, HeartbeatService>();
            }
            services.AddHostedServiceUsingContainer<TContainer, GracefulShutdownService>();

            return services;
        }

        public static IServiceCollection AddCoreSlateLogging(this IServiceCollection services,
            IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            services.AddLogging(lb => lb
                .ClearProviders()
                .AddSerilog(dispose: true));

            return services;
        }
    }
}
