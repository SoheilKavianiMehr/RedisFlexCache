using StackExchange.Redis;

namespace RedisFlexCache.Interfaces;

public interface IDatabaseProvider
{
    IDatabase GetDatabase();
}
