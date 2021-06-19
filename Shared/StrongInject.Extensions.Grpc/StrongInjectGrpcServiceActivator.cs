using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;

namespace StrongInject.Extensions.Grpc
{
    internal sealed class StrongInjectGrpcServiceActivator<
#if NET5_0
        [DynamicallyAccessedMembers(ServiceAccessibility)]
#endif
        TGrpcService> : IGrpcServiceActivator<TGrpcService> where TGrpcService : class
    {
#if NET5_0
        internal const DynamicallyAccessedMemberTypes ServiceAccessibility = DynamicallyAccessedMemberTypes.PublicConstructors;
#endif
        private static readonly Lazy<ObjectFactory> _objectFactory = new Lazy<ObjectFactory>(static () => ActivatorUtilities.CreateFactory(typeof(TGrpcService), Type.EmptyTypes));

        public GrpcActivatorHandle<TGrpcService> Create(IServiceProvider serviceProvider)
        {
            TGrpcService? service;

            var container = serviceProvider.GetService<IContainer<TGrpcService>>();
            if (container is not null)
            {
                //Resolve from service provider instead
                service = container.Resolve().Value;
            }
            else
            {
                service = serviceProvider.GetService<TGrpcService>();
            }
            
            if (service == null)
            {
                service = (TGrpcService)_objectFactory.Value(serviceProvider, Array.Empty<object>());
                return new GrpcActivatorHandle<TGrpcService>(service, created: true, state: null);
            }

            return new GrpcActivatorHandle<TGrpcService>(service, created: false, state: null);
        }

        public ValueTask ReleaseAsync(GrpcActivatorHandle<TGrpcService> service)
        {
            if (service.Instance == null)
            {
                throw new ArgumentException("Service instance is null.", nameof(service));
            }

            if (service.Created)
            {
                if (service.Instance is IAsyncDisposable asyncDisposableService)
                {
                    return asyncDisposableService.DisposeAsync();
                }

                if (service.Instance is IDisposable disposableService)
                {
                    disposableService.Dispose();
                    return default;
                }
            }

            return default;
        }
    }
}