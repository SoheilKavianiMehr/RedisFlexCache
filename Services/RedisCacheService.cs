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

        public RedisCacheService(IOptions<RedisCacheOptions> options, ICacheProvider cacheProvider, ILogger<RedisCacheService> logger)
        {
            _options = options.Value;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

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

        public Task RemoveAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            return _cacheProvider.RemoveAsync(Hash(key), ttl, cancellationToken);
        }

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

        public Task<long> SortedSetLengthAsync(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            return _cacheProvider.SortedSetLengthAsync(Hash(key), min, max);
        }

        public Task<bool> SortedSetAddAsync(string key, long member, double score)
        {
            return _cacheProvider.SortedSetAddAsync(Hash(key), member, score, CommandFlags.FireAndForget);
        }

        public Task<bool> KeyExpireAsync(string key, TimeSpan? expiry)
        {
            return _cacheProvider.KeyExpireAsync(Hash(key), expiry, CommandFlags.FireAndForget);
        }

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