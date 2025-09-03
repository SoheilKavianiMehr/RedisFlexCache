using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisFlexCache.Configuration;
using RedisFlexCache.Interfaces;
using RedisFlexCache.Services;
using StackExchange.Redis;
using System.Linq;

namespace RedisFlexCache.Provider;

/// <summary>
/// Provides Redis database instances with connection pooling support.
/// Manages multiple Redis connections for load balancing and high availability.
/// </summary>
public class RedisDatabaseProvider : IDatabaseProvider
{
    private readonly List<ConnectionMultiplexer> _connectionPool;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDatabaseProvider"/> class.
    /// </summary>
    /// <param name="options">The Redis cache configuration options.</param>
    /// <param name="logger">The logger instance.</param>
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

    /// <inheritdoc/>
    public IDatabase GetDatabase()
    {
        if (_connectionPool.Count == 0)
            throw new InvalidOperationException("No Redis connections available in the pool.");
            
        // Use modulo to ensure we don't go out of bounds
        var index = _random.Next(0, _connectionPool.Count);
        var multiplexer = _connectionPool[index];
        
        if (!multiplexer.IsConnected)
        {
            _logger.LogWarning("Selected Redis connection is not connected, attempting to reconnect...");
            // Try to find a connected multiplexer
            var connectedMultiplexer = _connectionPool.FirstOrDefault(c => c.IsConnected);
            if (connectedMultiplexer != null)
            {
                return connectedMultiplexer.GetDatabase();
            }
            _logger.LogError("No connected Redis instances available in the pool.");
        }
        
        return multiplexer.GetDatabase();
    }
}
