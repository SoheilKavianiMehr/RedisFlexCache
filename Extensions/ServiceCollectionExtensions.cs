using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using RedisFlexCache.Provider;
using RedisFlexCache.Services;
using StackExchange.Redis;

namespace RedisFlexCache.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> to register Redis cache services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Redis cache services to the dependency injection container with a connection string.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The Redis connection string.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            return services.AddRedisFlexCache(options =>
            {
                options.Connection = connectionString;
            }, lifetime);
        }

        /// <summary>
        /// Adds Redis cache services to the dependency injection container with configuration options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, Action<RedisCacheOptions> configureOptions, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);

            services.RegisterLocal(lifetime);

            return services;
        }

        /// <summary>
        /// Adds Redis cache services to the dependency injection container using configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionName">The configuration section name (default: "RedisCache").</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, IConfiguration configuration, string sectionName = "RedisCache", ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(sectionName))
                throw new ArgumentException("Section name cannot be null or empty.", nameof(sectionName));

            var section = configuration.GetSection(sectionName);
            if (!section.Exists())
                throw new InvalidOperationException($"Configuration section '{sectionName}' not found.");

            return services.AddRedisFlexCache(options => section.Bind(options), lifetime);
        }

        /// <summary>
        /// Adds a custom Redis cache service implementation to the dependency injection container.
        /// </summary>
        /// <typeparam name="TImplementation">The custom cache service implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache<TImplementation>(this IServiceCollection services, Action<RedisCacheOptions> configureOptions, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TImplementation : class, ICacheService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);

            services.RegisterLocal(lifetime);

            return services;
        }

        /// <summary>
        /// Adds Redis cache services with options validation to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <param name="validateOnStart">Whether to validate the configuration on application start.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheWithValidation(this IServiceCollection services, Action<RedisCacheOptions> configureOptions, bool validateOnStart = true, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            if (validateOnStart)
            {
                services.PostConfigure<RedisCacheOptions>(options => options.Validate());
            }
            
            return services.AddRedisFlexCache(configureOptions, lifetime);
        }

        /// <summary>
        /// Adds Redis cache services with scoped lifetime to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheScoped(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
        {
            return services.AddRedisFlexCache(configureOptions, ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds Redis cache services with transient lifetime to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheTransient(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
        {
            return services.AddRedisFlexCache(configureOptions, ServiceLifetime.Transient);
        }

        /// <summary>
        /// Registers Redis cache dependencies with the specified service lifetime.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="lifetime">The service lifetime to use for registration.</param>
        private static void RegisterLocal(this IServiceCollection services, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddLocalSingleton();
                    services.TryAddSingleton<ICacheService, RedisCacheService>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddLocalScoped();
                    services.TryAddScoped<ICacheService, RedisCacheService>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddLocalTransient();
                    services.TryAddTransient<ICacheService, RedisCacheService>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime specified.");
            }
        }

        /// <summary>
        /// Adds the local Redis cache dependencies with Singleton lifetime to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        private static IServiceCollection AddLocalSingleton(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, RedisDatabaseProvider>();
            services.AddSingleton<ICacheProvider, RedisCacheProvider>();
            
            // Register IDatabase factory that provides separate read/write databases
            services.AddTransient<IDatabase>(provider => 
            {
                var databaseProvider = provider.GetRequiredService<IDatabaseProvider>();
                return databaseProvider.GetDatabase();
            });

            return services;
        }

        /// <summary>
        /// Adds the local Redis cache dependencies with transient lifetime to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        private static IServiceCollection AddLocalTransient(this IServiceCollection services)
        {
            services.AddTransient<IDatabaseProvider, RedisDatabaseProvider>();
            services.AddTransient<ICacheProvider, RedisCacheProvider>();
            
            // Register IDatabase factory that provides separate read/write databases
            services.AddTransient<IDatabase>(provider => 
            {
                var databaseProvider = provider.GetRequiredService<IDatabaseProvider>();
                return databaseProvider.GetDatabase();
            });

            return services;
        }

        /// <summary>
        /// Adds the local Redis cache dependencies with scoped lifetime to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        private static IServiceCollection AddLocalScoped(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseProvider, RedisDatabaseProvider>();
            services.AddScoped<ICacheProvider, RedisCacheProvider>();
            
            // Register IDatabase factory that provides separate read/write databases
            services.AddScoped<IDatabase>(provider => 
            {
                var databaseProvider = provider.GetRequiredService<IDatabaseProvider>();
                return databaseProvider.GetDatabase();
            });

            return services;
        }
    }
}