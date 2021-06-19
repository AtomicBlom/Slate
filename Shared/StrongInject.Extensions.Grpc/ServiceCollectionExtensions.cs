using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;
using StrongInject.Extensions.Grpc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseStrongInjectForGrpcServiceResolution(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGrpcServiceActivator<>), typeof(StrongInjectGrpcServiceActivator<>));
            return services;
        }
    }
}
