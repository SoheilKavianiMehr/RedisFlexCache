using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using RedisFlexCache.Provider;
using RedisFlexCache.Services;

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
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            return services.AddRedisFlexCache(options =>
            {
                options.Connection = connectionString;
            });
        }

        /// <summary>
        /// Adds Redis cache services to the dependency injection container with configuration options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            services.AddLocal();
            services.TryAddSingleton<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Adds Redis cache services to the dependency injection container using configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionName">The configuration section name (default: "RedisCache").</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache(this IServiceCollection services, IConfiguration configuration, string sectionName = "RedisCache")
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

            services.Configure<RedisCacheOptions>(section);
            services.AddLocal();
            services.TryAddSingleton<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Adds a custom Redis cache service implementation to the dependency injection container.
        /// </summary>
        /// <typeparam name="TImplementation">The custom cache service implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCache<TImplementation>(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
            where TImplementation : class, ICacheService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            services.AddLocal();
            services.TryAddSingleton<ICacheService, TImplementation>();

            return services;
        }

        /// <summary>
        /// Adds Redis cache services with options validation to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <param name="validateOnStart">Whether to validate the configuration on application start.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheWithValidation(this IServiceCollection services, Action<RedisCacheOptions> configureOptions, bool validateOnStart = true)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            
            if (validateOnStart)
            {
                services.PostConfigure<RedisCacheOptions>(options => options.Validate());
            }
            
            services.AddLocal();
            services.TryAddSingleton<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Adds Redis cache services with scoped lifetime to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheScoped(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            services.AddLocal();
            services.TryAddScoped<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Adds Redis cache services with transient lifetime to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">An action to configure the Redis cache options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRedisFlexCacheTransient(this IServiceCollection services, Action<RedisCacheOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            services.AddLocal();
            services.TryAddTransient<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Adds the local Redis cache dependencies to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        private static IServiceCollection AddLocal(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, RedisDatabaseProvider>();
            services.AddScoped<ICacheProvider, RedisCacheProvider>();
            services.AddTransient(provider => provider.GetService<IDatabaseProvider>()?.GetDatabase());


            return services;
        }
    }
}