using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            return services;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Log.Logger.Fatal(e.ExceptionObject as Exception, "Fatal Unhandled Exception");
                Log.CloseAndFlush();
            }
            else
            {
                Log.Logger.Error(e.ExceptionObject as Exception, "Unhandled Exception");
            }
        }

        public static IServiceCollection AddCoreSlateLogging(this IServiceCollection services,
            IConfiguration configuration)
        {
            var applicationSessionId = Guid.NewGuid().ToString().Substring(24);
            var instanceId = configuration["Id"];

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationSessionId", applicationSessionId);

            if (!string.IsNullOrEmpty(instanceId))
            {
                loggerConfiguration = loggerConfiguration.Enrich.WithProperty("InstanceId", instanceId);
            }
            
            Log.Logger = loggerConfiguration
                .CreateLogger();

            services.AddLogging(lb => lb
                .ClearProviders()
                .AddSerilog(dispose: true));

            return services;
        }
    }
}
