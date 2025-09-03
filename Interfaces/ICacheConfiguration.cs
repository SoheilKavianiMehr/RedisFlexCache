using System;

namespace RedisFlexCache.Interfaces
{
    /// <summary>
    /// Defines the configuration contract for Redis cache settings.
    /// </summary>
    public interface ICacheConfiguration
    {
        /// <summary>
        /// Gets the Redis connection string.
        /// </summary>
        string Connection { get; }

        /// <summary>
        /// Gets the default expiration time for cached items.
        /// </summary>
        TimeSpan DefaultExpiration { get; }

        /// <summary>
        /// Gets the optional key prefix for all cache keys.
        /// </summary>
        string? KeyPrefix { get; }

        /// <summary>
        /// Gets the number of connections to maintain in the connection pool.
        /// </summary>
        int ConnectionCount { get; }

        /// <summary>
        /// Gets a value indicating whether compression is enabled for cached values.
        /// </summary>
        bool EnableCompression { get; }
        /// <summary>
        /// Gets a value indicating whether to hash cache keys using SHA256.
        /// </summary>
        bool EnableHashKey { get; }

        /// <summary>
        /// Gets the connection timeout for Redis operations.
        /// </summary>
        int ConnectionTimeout { get; }

        /// <summary>
        /// Gets the synchronous operation timeout.
        /// </summary>
        int CommandTimeout { get; }
        /// <summary>
        /// Gets the maximum allowed length for cache keys.
        /// </summary>
        int MaxKeyLength { get; }

        /// <summary>
        /// Gets the username for Redis authentication.
        /// </summary>
        string Username { get; }
        
        /// <summary>
        /// Gets the password for Redis authentication.
        /// </summary>
        string Password { get; }
        
        /// <summary>
        /// Gets a value indicating whether database caching is active.
        /// </summary>
        bool IsDbCachedActive { get; }
    }
}