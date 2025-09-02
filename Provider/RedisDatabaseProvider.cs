using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using RedisFlexCache.Services;
using StackExchange.Redis;

namespace RedisFlexCache.Provider;

public class RedisDatabaseProvider : IDatabaseProvider
{
    private readonly List<ConnectionMultiplexer> _connectionPool;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly Random _random;

    public RedisDatabaseProvider(IOptions<RedisCacheOptions> options, ILogger<RedisCacheService> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _options.Validate();

        _random = new Random();
        _connectionPool = new List<ConnectionMultiplexer>();

        var connections = options.Value.Connection.Split(";");

        for (int i = 0; i < options.Value.ConnectionCount; i++)
        {
            try
            {
                var config = new ConfigurationOptions();

                foreach (var endpoint in connections)
                {
                    config.EndPoints.Add(endpoint);
                }

                config.AbortOnConnectFail = false;
                config.ConnectRetry = 10;
                config.ConnectTimeout = _options.ConnectionTimeout;
                config.SyncTimeout = _options.CommandTimeout;
                config.ReconnectRetryPolicy = new ExponentialRetry(1000);
                config.User = options.Value.Username;
                config.Password = options.Value.Password;

                var connection = ConnectionMultiplexer.Connect(config);

                connection.ConnectionFailed += (sender, args) =>
                {
                    _logger.LogError("Redis connection failed: {Exception}", args.Exception?.Message);
                };

                connection.ConnectionRestored += (sender, args) =>
                {
                    _logger.LogInformation("Redis connection restored");
                };

                _logger.LogInformation("Redis connection established successfully");

                _connectionPool.Add(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Redis connection");
                throw;
            }
        }

    }

    public IDatabase GetDatabase()
    {
        ConnectionMultiplexer multiplexer = _connectionPool[_random.Next(0, _options.ConnectionCount - 1)];
        return multiplexer.GetDatabase();
    }
}
