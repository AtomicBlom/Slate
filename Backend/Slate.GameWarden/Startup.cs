using System;
using System.Diagnostics;
using System.IO.Compression;
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
using Slate.Backend.Shared;
using Slate.GameWarden.Game;
using Slate.GameWarden.ServiceLocation;
using Slate.Networking.External.Protocol;
using StrongInject;

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
            services.AddCoreSlateServices<GameContainer>(_configuration);

            Log.Logger.Information("GameWarden Starting");

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
            services.AddSingleton<IContainer<Func<Guid, PlayerConnection>>>(sp => sp.GetRequiredService<GameContainer>());
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
}

