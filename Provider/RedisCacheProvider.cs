using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using StackExchange.Redis;

namespace RedisFlexCache.Provider
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly RedisCacheOptions _options;
        private readonly ILogger<RedisCacheProvider> _logger;
        private readonly IDatabase _readDatabase;
        private readonly IDatabase _writeDatabase;
        private readonly Random random = new();
        private readonly MessagePackSerializerOptions _messagePackOptions;


        public RedisCacheProvider(IOptions<RedisCacheOptions> options, ILogger<RedisCacheProvider> logger, IDatabase readDatabase, IDatabase writeDatabase)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _readDatabase = readDatabase;
            _writeDatabase = writeDatabase;

            var resolver = CompositeResolver.Create(
                StandardResolver.Instance,
                ContractlessStandardResolver.Instance
                );

            _messagePackOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            if (_options.EnableCompression)
            {
                _messagePackOptions = _messagePackOptions
                    .WithCompression(MessagePackCompression.Lz4BlockArray);
            }
        }

        public async Task StoreAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);
                var serializedValue = SerializeValue(value);
                var exp = PrepareTtl(ttl);

                await _writeDatabase.StringSetAsync(key, serializedValue, exp, flags: CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "StoreAsync Has Exception");
                throw new RedisException("Redis cache provider exception on Store", e.InnerException);
            }
        }

        public void Store<T>(string key, T value, TimeSpan? ttl = null)
        {
            StoreAsync(key, value, ttl).GetAwaiter().GetResult();
        }

        public async Task<T?> FetchAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);
                var value = await _readDatabase.StringGetAsync(key);

                if (value.IsNull || !value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default(T);
                }

                return DeserializeValue<T>(value!);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "FetchAsync Has Exception");
                throw new RedisException("Redis cache provider exception on Fetch", e.InnerException);
            }

        }

        public T? Fetch<T>(string key)
        {
            return FetchAsync<T>(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, TimeSpan? removeAt = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);

                if (removeAt.HasValue)
                {
                    await _writeDatabase.KeyExpireAsync(key, removeAt);
                }
                else
                {
                    await _writeDatabase.KeyDeleteAsync(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key from cache: {Key}", key);
                throw new RedisException("Redis cache provider exception on Remove", ex.InnerException);
            }
        }

        public void Remove(string key, TimeSpan? removeAt = null)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);
                return await _readDatabase.KeyExistsAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
                return false;
            }
        }

        public bool Exists(string key)
        {
            return ExistsAsync(key).GetAwaiter().GetResult();
        }

        /// <summary>
        /// to search in redis keys
        /// </summary>
        /// <param name="scan">must be 0</param>
        /// <param name="match"> wildcard use for search in keys</param>
        /// <param name="count">count to return keys</param>
        /// <returns>list of keys</returns>
        public async Task<List<string>> ScanKeysAsync(string scan, string match, string count)
        {
            var schemas = new List<string>();
            int nextCursor = 0;
            do
            {
                RedisResult redisResult = await _writeDatabase.ExecuteAsync("SCAN", nextCursor.ToString(), "MATCH", match, "COUNT", count);
                var innerResult = (RedisResult[])redisResult;

                nextCursor = int.Parse((string)innerResult[0]);

                List<string> resultLines = ((string[])innerResult[1]).ToList();
                schemas.AddRange(resultLines);
            }
            while (nextCursor != 0);

            return schemas;
        }

        public async Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity,
            double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            try
            {
                return await _readDatabase.SortedSetLengthAsync(key, min, max, exclude, flags);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while getting the sorted set length for key: {Key}", key);
                throw new RedisException($"Error occurred while getting the sorted set length for key: {key}", e.InnerException);
            }
        }

        public async Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.FireAndForget)
        {
            try
            {
                return await _writeDatabase.SortedSetAddAsync(key, member, score, flags);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding member to sorted set in Redis");
                throw new RedisException("Error adding member to sorted set in Redis", e.InnerException);
            }
        }

        public async Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.FireAndForget)
        {
            try
            {
                return await _writeDatabase.KeyExpireAsync(key, expiry, flags);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while setting the expiration for key {Key}", key);
                throw new RedisException($"An error occurred while setting the expiration for key {key}", e.InnerException);
            }
        }

        public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);
                return await _readDatabase.KeyTimeToLiveAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
                return null;
            }
        }

        public TimeSpan? GetTimeToLive(string key)
        {
            try
            {
                var redisKey = BuildKey(key);
                return _readDatabase.KeyTimeToLive(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
                return null;
            }
        }

        public async Task<bool> RefreshAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = BuildKey(key);
                return await _writeDatabase.KeyExpireAsync(redisKey, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing expiration for key: {Key}", key);
                return false;
            }
        }

        public bool Refresh(string key, TimeSpan expiration)
        {
            try
            {
                var redisKey = BuildKey(key);
                return _writeDatabase.KeyExpire(redisKey, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing expiration for key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Builds the Redis key with optional prefix.
        /// </summary>
        /// <param name="key">The original key.</param>
        /// <returns>The Redis key with prefix if configured.</returns>
        private string BuildKey(string key)
        {
            var finalKey = string.IsNullOrEmpty(_options.KeyPrefix) ? key : $"{_options.KeyPrefix}:{key}";
            if (finalKey.Length > _options.MaxKeyLength)
            {
                _logger.LogError("Redis key too long ({Length} chars). Limit is {Limit}. Key={Key}", finalKey.Length, _options.MaxKeyLength, finalKey);
                throw new ArgumentException($"Redis key exceeds maximum length of {_options.MaxKeyLength} characters", nameof(key));
            }
            return finalKey;
        }

        private TimeSpan PrepareTtl(TimeSpan? ttl)
        {
            TimeSpan exp = ttl ?? _options.DefaultExpiration;
            int seconds = random.Next(10, 121);
            return exp.Add(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Serializes a value for storage in Redis.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized value.</returns>
        private byte[]? SerializeValue<T>(T value)
        {
            if (value == null) return null;

            var result = MessagePackSerializer.Serialize(value, _messagePackOptions);

            return result;
        }

        /// <summary>
        /// Deserializes a value from Redis storage.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The serialized value.</param>
        /// <returns>The deserialized value.</returns>
        private T? DeserializeValue<T>(byte[] value)
        {
            if (value is null) return default;

            try
            {
                return MessagePackSerializer.Deserialize<T>(value, _messagePackOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing cached value");
                return default;
            }
        }
    }
}
