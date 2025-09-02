using System;

namespace RedisFlexCache.Models
{
    /// <summary>
    /// Represents the result of a cache operation.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    public class CacheResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheResult{T}"/> class.
        /// </summary>
        /// <param name="value">The cached value.</param>
        /// <param name="hasValue">Indicates whether the cache contained a value.</param>
        /// <param name="expiration">The expiration time of the cached value.</param>
        public CacheResult(T? value, bool hasValue, DateTime? expiration = null)
        {
            Value = value;
            HasValue = hasValue;
            Expiration = expiration;
        }

        /// <summary>
        /// Gets or sets the cached value.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the cache contains a value for the key.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the expiration time of the cached value.
        /// </summary>
        public DateTime? Expiration { get; }

        /// <summary>
        /// Gets a value indicating whether the cached value has expired.
        /// </summary>
        public bool IsExpired => Expiration.HasValue && Expiration.Value <= DateTime.UtcNow;

        /// <summary>
        /// Creates a cache result indicating that no value was found.
        /// </summary>
        /// <returns>A cache result with no value.</returns>
        public static CacheResult<T> Miss() => new(default, false);

        /// <summary>
        /// Creates a cache result with the specified value.
        /// </summary>
        /// <param name="value">The cached value.</param>
        /// <param name="expiration">The expiration time of the cached value.</param>
        /// <returns>A cache result with the specified value.</returns>
        public static CacheResult<T> Hit(T value, DateTime? expiration = null) => new(value, true, expiration);
    }

    /// <summary>
    /// Represents cache statistics and metrics.
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Gets or sets the total number of cache hits.
        /// </summary>
        public long Hits { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache misses.
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache operations.
        /// </summary>
        public long TotalOperations { get; set; }

        /// <summary>
        /// Gets the cache hit ratio as a percentage (0.0 to 1.0).
        /// </summary>
        public double HitRatio => TotalOperations > 0 ? (double)Hits / TotalOperations * 100 : 0;

        /// <summary>
        /// Gets or sets the total number of cache operation errors.
        /// </summary>
        public long Errors { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when statistics were last reset.
        /// </summary>
        public DateTime LastReset { get; set; } = DateTime.UtcNow;
    }
}