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
using Serilog;
using Slate.Backend.Shared;

namespace Slate.Snowglobe
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
            services.AddCoreSlateServices<SnowglobeContainer>(_configuration);

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
            
            services.ReplaceWithSingletonServiceUsingContainer<SnowglobeContainer, IGameWardenService>();

            //services.AddSingleton<IContainer<Func<Guid, PlayerConnection>>>(sp => sp.GetRequiredService<GameContainer>());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<IGameWardenService>();
            });
        }
    }
}
