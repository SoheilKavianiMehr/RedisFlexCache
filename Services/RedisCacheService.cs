using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace RedisFlexCache.Services
{
    /// <summary>
    /// Redis implementation of the <see cref="ICacheService"/> interface.
    /// Provides comprehensive caching functionality with Redis backend support.
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly RedisCacheOptions _options;
        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger<RedisCacheService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
        /// </summary>
        /// <param name="options">The Redis cache configuration options.</param>
        /// <param name="cacheProvider">The cache provider for Redis operations.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        public RedisCacheService(IOptions<RedisCacheOptions> options, ICacheProvider cacheProvider, ILogger<RedisCacheService> logger)
        {
            _options = options.Value;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves a value from the cache by key, or fetches it from storage if not cached.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="getObjectFromStorageFunc">Function to retrieve the object from storage if not in cache.</param>
        /// <param name="ttl">Time to live for the cached value.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The cached or retrieved value.</returns>
        public async Task<T?> GetAsync<T>(string key, Func<Task<T>> getObjectFromStorageFunc, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            if (!_options.IsDbCachedActive)
                return await getObjectFromStorageFunc();

            try
            {
                key = Hash(key);

                var cacheObject = await _cacheProvider.FetchAsync<T>(key);

                if (cacheObject != null)
                    return cacheObject;

                var result = await getObjectFromStorageFunc();

                if (result == null)
                    return default(T);

                await SetAsync(key, result, ttl);

                return result;

            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
            }

            return await getObjectFromStorageFunc();
        }
        /// <inheritdoc/>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.FetchAsync<T>(Hash(key));
        }

        /// <inheritdoc/>
        public T? Get<T>(string key)
        {
            return GetAsync<T>(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.StoreAsync(Hash(key), value, ttl, cancellationToken);
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, TimeSpan? ttl = null)
        {
            SetAsync(key, value, ttl).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.RemoveAsync(Hash(key), ttl, cancellationToken);
        }

        /// <inheritdoc/>
        public void Remove(string key, TimeSpan? ttl = null)
        {
            RemoveAsync(key, ttl).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.ExistsAsync(Hash(key), cancellationToken);
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            return ExistsAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.GetTimeToLiveAsync(Hash(key), cancellationToken);
        }

        /// <inheritdoc/>
        public TimeSpan? GetTimeToLive(string key)
        {
            return GetTimeToLiveAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task<bool> RefreshAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.RefreshAsync(Hash(key), ttl, cancellationToken);
        }

        /// <inheritdoc/>
        public bool Refresh(string key, TimeSpan ttl)
        {
            return RefreshAsync(key, ttl).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously gets the length of a sorted set within the specified score range.
        /// </summary>
        /// <param name="key">The cache key for the sorted set.</param>
        /// <param name="min">The minimum score (inclusive). Default is negative infinity.</param>
        /// <param name="max">The maximum score (inclusive). Default is positive infinity.</param>
        /// <returns>The number of elements in the sorted set within the specified range.</returns>
        public Task<long> SortedSetLengthAsync(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            return _cacheProvider.SortedSetLengthAsync(Hash(key), min, max);
        }

        /// <summary>
        /// Asynchronously adds a member with a score to a sorted set.
        /// </summary>
        /// <param name="key">The cache key for the sorted set.</param>
        /// <param name="member">The member to add to the sorted set.</param>
        /// <param name="score">The score associated with the member.</param>
        /// <returns>True if the member was added; false if it already existed and the score was updated.</returns>
        public Task<bool> SortedSetAddAsync(string key, long member, double score)
        {
            return _cacheProvider.SortedSetAddAsync(Hash(key), member, score, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Asynchronously sets the expiration time for a key.
        /// </summary>
        /// <param name="key">The cache key to set expiration for.</param>
        /// <param name="expiry">The expiration time. If null, removes the expiration.</param>
        /// <returns>True if the expiration was set successfully; otherwise, false.</returns>
        public Task<bool> KeyExpireAsync(string key, TimeSpan? expiry)
        {
            return _cacheProvider.KeyExpireAsync(Hash(key), expiry, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Hashes the input string using SHA256 if hash key is enabled in options.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <returns>The hashed string if hashing is enabled; otherwise, the original input.</returns>
        private string Hash(string input)
        {
            if (!_options.EnableHashKey)
            {
                return input;
            }

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}