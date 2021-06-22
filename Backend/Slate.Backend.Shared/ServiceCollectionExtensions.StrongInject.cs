using Microsoft.Extensions.DependencyInjection.Extensions;
using StrongInject;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTransientServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            AddTransientServiceUsingContainer<TContainer, TService, TService>(services);
        }

        public static void AddTransientServiceUsingContainer<TContainer, TService, TServiceImpl>(this IServiceCollection services) where TContainer : class, IContainer<TServiceImpl> where TService : class where TServiceImpl : TService
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TServiceImpl>>(sp => sp.GetRequiredService<TContainer>());
            services.AddTransient(x => x.GetRequiredService<TContainer>().Resolve());
            services.AddTransient<TService>(x => x.GetRequiredService<Owned<TServiceImpl>>().Value);
        }

        public static void AddTransientServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddTransient(x => container.Resolve());
            services.AddTransient(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddScopedServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddScoped(x => x.GetRequiredService<TContainer>().Resolve());
            services.AddScoped(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddScopedServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddScoped(x => container.Resolve());
            services.AddScoped(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddSingletonServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddSingleton(x => x.GetRequiredService<TContainer>().Resolve());
            services.AddSingleton(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddSingletonServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddSingleton(x => container.Resolve());
            services.AddSingleton(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddTransientServiceUsingScopedContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddScoped<TContainer, TContainer>();
            services.TryAddScoped<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddTransient(x => x.GetRequiredService<TContainer>().Resolve());
            services.AddTransient(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        public static void AddScopedServiceUsingScopedContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddScoped<TContainer, TContainer>();
            services.TryAddScoped<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddScoped(x => x.GetRequiredService<TContainer>().Resolve());
            services.AddScoped(x => x.GetRequiredService<Owned<TService>>().Value);
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> transiently using <typeparamref name="TContainer"/>.
        /// </summary>
        public static void ReplaceWithTransientServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddTransient(x => x.GetRequiredService<TContainer>().Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Transient));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> using <paramref name="container"/>.
        /// </summary>
        public static void ReplaceWithTransientServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddTransient(x => container.Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Transient));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> transiently using <typeparamref name="TContainer"/>.
        /// </summary>
        public static void ReplaceWithScopedServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddScoped(x => x.GetRequiredService<TContainer>().Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Scoped));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> using <paramref name="container"/>.
        /// </summary>
        public static void ReplaceWithScopedServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddScoped(x => container.Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Scoped));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> transiently using <typeparamref name="TContainer"/>.
        /// </summary>
        public static void ReplaceWithSingletonServiceUsingContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddSingleton<TContainer, TContainer>();
            services.TryAddSingleton<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddSingleton(x => x.GetRequiredService<TContainer>().Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Singleton));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> using <paramref name="container"/>.
        /// </summary>
        public static void ReplaceWithSingletonServiceUsingContainer<TService>(this IServiceCollection services, IContainer<TService> container) where TService : class
        {
            services.AddSingleton(x => container.Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Singleton));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> transiently using <typeparamref name="TContainer"/>.
        /// </summary>
        public static void ReplaceWithTransientServiceUsingScopedContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddScoped<TContainer, TContainer>();
            services.TryAddScoped<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddTransient(x => x.GetRequiredService<TContainer>().Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Transient));
        }

        /// <summary>
        /// Removes the first service type in <paramref name="services"/> with the same service type as <typeparamref name="TService"/>
        /// and adds a <see cref="ServiceDescriptor"/> to the collection which resolves <typeparamref name="TService"/> transiently using <typeparamref name="TContainer"/>.
        /// </summary>
        public static void ReplaceWithScopedServiceUsingScopedContainer<TContainer, TService>(this IServiceCollection services) where TContainer : class, IContainer<TService> where TService : class
        {
            services.TryAddScoped<TContainer, TContainer>();
            services.TryAddScoped<IContainer<TService>>(sp => sp.GetRequiredService<TContainer>());
            services.AddScoped(x => x.GetRequiredService<TContainer>().Resolve());
            services.Replace(new ServiceDescriptor(typeof(TService), x => x.GetRequiredService<Owned<TService>>().Value, ServiceLifetime.Scoped));
        }
    }
}