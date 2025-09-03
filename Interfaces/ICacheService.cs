namespace RedisFlexCache.Interfaces
{
    /// <summary>
    /// Defines a flexible caching service interface with Redis backend support.
    /// Provides both synchronous and asynchronous operations for cache management.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Asynchronously retrieves a value from the cache by key, or fetches it from storage if not cached.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="getObjectFromStorageFunc">Function to retrieve the object from storage if not in cache.</param>
        /// <param name="ttl">Time to live for the cached value.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The cached or retrieved value.</returns>
        Task<T?> GetAsync<T>(
           string key,
           Func<Task<T>> getObjectFromStorageFunc,
           TimeSpan ttl,
           CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves a value from the cache by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The cached value if found; otherwise, null.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously retrieves a value from the cache by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value if found; otherwise, null.</returns>
        T? Get<T>(string key);

        /// <summary>
        /// Asynchronously stores a value in the cache with an optional ttl time.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="ttl">Optional ttl time. If null, uses default ttl.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>True if the value was successfully cached; otherwise, false.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously stores a value in the cache with an optional ttl time.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="ttl">Optional ttl time. If null, uses default ttl.</param>
        /// <returns>True if the value was successfully cached; otherwise, false.</returns>
        void Set<T>(string key, T value, TimeSpan? ttl = null);

        /// <summary>
        /// Asynchronously removes a value from the cache by key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <param name="ttl">Optional time-to-live for delayed removal.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>True if the key was found and removed; otherwise, false.</returns>
        Task RemoveAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously removes a value from the cache by key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <param name="ttl">Optional time-to-live for delayed removal.</param>
        /// <returns>True if the key was found and removed; otherwise, false.</returns>
        void Remove(string key, TimeSpan? ttl = null);

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
        /// <returns>The remaining time to live, or null if the key doesn't exist or has no ttl.</returns>
        Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);


        /// <summary>
        /// Synchronously gets the remaining time to live for a key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The remaining time to live, or null if the key doesn't exist or has no ttl.</returns>
        TimeSpan? GetTimeToLive(string key);

        /// <summary>
        /// Asynchronously refreshes the ttl time of a key.
        /// </summary>
        /// <param name="key">The cache key to refresh.</param>
        /// <param name="ttl">The new ttl time.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>True if the ttl was updated successfully.</returns>
        Task<bool> RefreshAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously refreshes the ttl time of a key.
        /// </summary>
        /// <param name="key">The cache key to refresh.</param>
        /// <param name="ttl">The new ttl time.</param>
        /// <returns>True if the ttl was updated successfully.</returns>
        bool Refresh(string key, TimeSpan ttl);

        /// <summary>
        /// Asynchronously gets the length of a sorted set within the specified score range.
        /// </summary>
        /// <param name="key">The cache key for the sorted set.</param>
        /// <param name="min">The minimum score (inclusive). Default is negative infinity.</param>
        /// <param name="max">The maximum score (inclusive). Default is positive infinity.</param>
        /// <returns>The number of elements in the sorted set within the specified range.</returns>
        Task<long> SortedSetLengthAsync(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity);
        
        /// <summary>
        /// Asynchronously adds a member with a score to a sorted set.
        /// </summary>
        /// <param name="key">The cache key for the sorted set.</param>
        /// <param name="member">The member to add to the sorted set.</param>
        /// <param name="score">The score associated with the member.</param>
        /// <returns>True if the member was added; false if it already existed and the score was updated.</returns>
        Task<bool> SortedSetAddAsync(string key, long member, double score);
        
        /// <summary>
        /// Asynchronously sets the expiration time for a key.
        /// </summary>
        /// <param name="key">The cache key to set expiration for.</param>
        /// <param name="expiry">The expiration time. If null, removes the expiration.</param>
        /// <returns>True if the expiration was set successfully; otherwise, false.</returns>
        Task<bool> KeyExpireAsync(string key, TimeSpan? expiry);
    }
}