using StackExchange.Redis;
using System.ComponentModel;

namespace RedisFlexCache.Interfaces;

public interface ICacheProvider
{
    /// <summary>
    /// fetch data from redis based on given key
    /// </summary>
    /// <typeparam name="T">object type</typeparam>
    /// <param name="key">redis key</param>
    /// <returns></returns>
    Task<T?> FetchAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// fetch data from redis based on given key
    /// </summary>
    /// <typeparam name="T">object type</typeparam>
    /// <param name="key">redis key</param>
    /// <returns></returns>
    T? Fetch<T>(string key);

    /// <summary>
    /// store data on redis
    /// </summary>
    /// <typeparam name="T">object type</typeparam>
    /// <param name="key">redis key</param>
    /// <param name="value">value</param>
    /// <param name="expiration">expiration</param>
    /// <param name="ttl">time to live</param>
    /// <returns></returns>
    Task StoreAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// store data on redis
    /// </summary>
    /// <typeparam name="T">object type</typeparam>
    /// <param name="key">redis key</param>
    /// <param name="value">value</param>
    /// <param name="expiration">expiration</param>
    /// <param name="ttl">time to live</param>
    /// <returns></returns>
    void Store<T>(string key, T value, TimeSpan? ttl = null);

   

    /// <summary>
    /// remove value from redis based on given key
    /// </summary>
    /// <param name="key">redis key</param>
    /// <param name="removeAt"></param>
    /// <returns></returns>
    Task RemoveAsync(string key, TimeSpan? removeAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// remove value from redis based on given key
    /// </summary>
    /// <param name="key">redis key</param>
    /// <param name="removeAt"></param>
    /// <returns></returns>
    void Remove(string key, TimeSpan? removeAt = null);

    /// <summary>
    /// Asynchronously checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    bool Exists(string key);

    /// <summary>
    /// Asynchronously gets the remaining time to live for a key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The remaining time to live, or null if the key doesn't exist or has no expiration.</returns>
    Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously gets the remaining time to live for a key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The remaining time to live, or null if the key doesn't exist or has no expiration.</returns>
    TimeSpan? GetTimeToLive(string key);

    /// <summary>
    /// Asynchronously refreshes the expiration time of a key.
    /// </summary>
    /// <param name="key">The cache key to refresh.</param>
    /// <param name="expiration">The new expiration time.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>True if the expiration was updated successfully.</returns>
    Task<bool> RefreshAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously refreshes the expiration time of a key.
    /// </summary>
    /// <param name="key">The cache key to refresh.</param>
    /// <param name="expiration">The new expiration time.</param>
    /// <returns>True if the expiration was updated successfully.</returns>
    bool Refresh(string key, TimeSpan expiration);

    /// <summary>
    /// to search in redis keys
    /// </summary>
    /// <param name="scan">must be 0</param>
    /// <param name="match"> wildcard use for search in keys</param>
    /// <param name="count">count to return keys</param>
    /// <returns>list of keys</returns>
    Task<List<string>> ScanKeysAsync(string scan, string match, string count);

    /// <summary>
    /// Returns the sorted set cardinality (number of elements) of the sorted set stored at key.
    /// </summary>
    /// <param name="key">The key of the sorted set.</param>
    /// <param name="min">The min score to filter by (defaults to negative infinity).</param>
    /// <param name="max">The max score to filter by (defaults to positive infinity).</param>
    /// <param name="exclude">Whether to exclude <paramref name="min"/> and <paramref name="max"/> from the range check (defaults to both inclusive).</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>The cardinality (number of elements) of the sorted set, or 0 if key does not exist.</returns>
    /// <remarks><seealso href="https://redis.io/commands/zcard"/></remarks>
    Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);

    /// <inheritdoc cref="SortedSetAddAsync(RedisKey,RedisValue,double,CommandFlags)" />
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags);

    /// <summary>
    /// Set a timeout on <paramref name="key"/>.
    /// After the timeout has expired, the key will automatically be deleted.
    /// A key with an associated timeout is said to be volatile in Redis terminology.
    /// </summary>
    /// <param name="key">The key to set the expiration for.</param>
    /// <param name="expiry">The timeout to set.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns><see langword="true"/> if the timeout was set. <see langword="false"/> if key does not exist or the timeout could not be set.</returns>
    /// <remarks>
    /// If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST command was invoked on key.
    /// <para>
    /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten.
    /// So, if key already has an associated timeout, it will do nothing and return 0.
    /// </para>
    /// <para>
    /// Since Redis 2.1.3, you can update the timeout of a key.
    /// It is also possible to remove the timeout using the PERSIST command.
    /// See the page on key expiry for more information.
    /// </para>
    /// <para>
    /// <seealso href="https://redis.io/commands/expire"/>,
    /// <seealso href="https://redis.io/commands/pexpire"/>,
    /// <seealso href="https://redis.io/commands/persist"/>
    /// </para>
    /// </remarks>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags);
}
