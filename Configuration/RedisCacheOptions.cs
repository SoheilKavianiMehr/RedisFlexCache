using System;
using RedisFlexCache.Interfaces;

namespace RedisFlexCache.Configuration
{
    /// <summary>
    /// Configuration options for Redis cache implementation.
    /// </summary>
    public class RedisCacheOptions : ICacheConfiguration
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        public string Connection { get; set; } = "localhost:6379";

        /// <summary>
        /// Gets or sets the default expiration time for cached items.
        /// </summary>
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets the optional key prefix for all cache keys.
        /// </summary>
        public string? KeyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the number of connections to maintain in the connection pool.
        /// </summary>
        public int ConnectionCount { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether compression is enabled for cached values.
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Gets or sets the connection timeout for Redis operations.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the synchronous operation timeout.
        /// </summary>
        public int CommandTimeout { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the username for Redis authentication.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password for Redis authentication.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether database caching is active.
        /// </summary>
        public bool IsDbCachedActive { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum allowed length for cache keys.
        /// </summary>
        public int MaxKeyLength { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value indicating whether to hash cache keys using SHA256.
        /// </summary>
        public bool EnableHashKey { get; set; } = false;

        /// <summary>
        /// Validates the configuration options and throws exceptions for invalid settings.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when configuration values are invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Connection))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(Connection));

            if (ConnectionCount < 1)
                throw new ArgumentException("Database number cannot be negative.", nameof(ConnectionCount));

            if (ConnectionTimeout <= 0)
                throw new ArgumentException("Connection timeout must be greater than zero.", nameof(ConnectionTimeout));

            if (CommandTimeout <= 0)
                throw new ArgumentException("Command timeout must be greater than zero.", nameof(CommandTimeout));
        }
    }
}