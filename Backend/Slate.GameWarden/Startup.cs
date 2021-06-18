using System;
using System.IO.Compression;
using MessagePipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;
using Slate.GameWarden.Game;
using Slate.GameWarden.Services;
using Slate.Networking.External.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.GameWarden
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
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


            /// GRPC
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<IGameService, GameService>();

            // RABBITMQ
            services.AddSingleton<IRabbitSettings>(sp =>
                sp.GetRequiredService<IConfiguration>().GetSection(RabbitSettings.SectionName).Get<RabbitSettings>());
            services.AddSingleton<IRabbitClient, RabbitClient>();
            services.AddScoped(sp => sp.GetRequiredService<IRabbitClient>().CreateRPCServer());
            services.AddScoped(sp => sp.GetRequiredService<IRabbitClient>().CreateRPCClient());

            services.AddSingleton<IPlayerLocator, PlayerLocator>();
            services.AddScoped<IPlayerService, CellPlayerService>();
            services.AddScoped<Func<Guid, IServiceScope, CharacterCoordinator>>(sp => 
                (id, scope) =>
                {
                    var character = new CharacterCoordinator(
                        id, 
                        scope,
                        sp.GetServices<IPlayerService>());
                    character.StartCoordinating();
                    return character;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<AuthorizationService>();
                endpoints.MapGrpcService<AccountService>();
                endpoints.MapGrpcService<GameService>();
            });
        }
    }
}