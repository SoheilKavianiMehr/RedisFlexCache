using StackExchange.Redis;

namespace RedisFlexCache.Interfaces;

/// <summary>
/// Defines the contract for providing Redis database instances.
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Gets the Redis database instance for cache operations.
    /// </summary>
    /// <returns>The Redis database instance.</returns>
    IDatabase GetDatabase();
}
