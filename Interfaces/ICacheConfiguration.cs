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

        int ConnectionCount { get; }

        /// <summary>
        /// Gets a value indicating whether compression is enabled for cached values.
        /// </summary>
        bool EnableCompression { get; }
        bool EnableHashKey { get; }

        /// <summary>
        /// Gets the connection timeout for Redis operations.
        /// </summary>
        int ConnectionTimeout { get; }

        /// <summary>
        /// Gets the synchronous operation timeout.
        /// </summary>
        int CommandTimeout { get; }
        int MaxKeyLength { get; }

        string Username { get; }
        string Password { get; }
        bool IsDbCachedActive { get; }
    }
}