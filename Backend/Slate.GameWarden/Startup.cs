using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;
using Serilog;
using Slate.GameWarden.Game;
using Slate.GameWarden.ServiceLocation;
using Slate.Networking.External.Protocol;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;
using StrongInject;
using Exception = System.Exception;
using ILogger = Serilog.ILogger;

namespace Slate.GameWarden
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            Log.Logger.Information("GameWarden Starting");

            services.AddLogging(lb => lb
                .ClearProviders()
                .AddSerilog(dispose: true));
            services.UseStrongInjectForGrpcServiceResolution();
            services.AddMessagePipe();
            services.AddCodeFirstGrpc(config =>
            {
                config.ResponseCompressionLevel = CompressionLevel.Optimal;
                config.EnableDetailedErrors = true;
            });
            services.TryAddSingleton(BinderConfiguration.Create(binder: new ServiceBinderWithServiceResolutionFromServiceCollection(services)));
            services.AddCodeFirstGrpcReflection();

            services.AddAuthorization();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:8001";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            services.ReplaceWithSingletonServiceUsingContainer<GameContainer, IAuthorizationService>();
            services.ReplaceWithSingletonServiceUsingContainer<GameContainer, IAccountService>();
            services.ReplaceWithSingletonServiceUsingContainer<GameContainer, IGameService>();
            services.AddSingleton<IContainer<Func<Guid, CharacterCoordinator>>>(sp => sp.GetRequiredService<GameContainer>());
            if (bool.TryParse(_configuration["UseHeartbeat"], out var useHeartbeat) && useHeartbeat)
            {
                services.AddTransientServiceUsingContainer<GameContainer, IHostedService, HeartbeatService>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<IAuthorizationService>();
                endpoints.MapGrpcService<IAccountService>();
                endpoints.MapGrpcService<IGameService>();
            });
        }
    }

    public class HeartbeatService : IHostedService
    {
        private readonly IRPCClient _rpcClient;
        private readonly ILogger _logger;
        private bool _running;

        public HeartbeatService(IRPCClient rpcClient, ILogger logger)
        {
            _rpcClient = rpcClient;
            _logger = logger.ForContext<HeartbeatService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Starting HeartbeatService");
            Task.Run(async () =>
            {
                while (_running)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _rpcClient.CallAsync<HeartbeatRequest, HeartbeatResponse>(new HeartbeatRequest());
                    }
                    catch (Exception e)
                    {
                        Environment.Exit(-1);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _running = false;
            return Task.CompletedTask;
        }
    }
}

