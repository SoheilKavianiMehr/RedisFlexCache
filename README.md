# RedisFlexCache

A flexible and easy-to-use Redis caching library for .NET applications with support for dependency injection, configuration patterns, and advanced caching strategies.

## Features

- 🚀 **High Performance**: Built on top of StackExchange.Redis for optimal performance
- 🔧 **Easy Configuration**: Simple setup with .NET configuration patterns
- 💉 **Dependency Injection**: Full support for .NET DI container
- 🔄 **Async/Sync Support**: Both asynchronous and synchronous operations
- 🗜️ **Compression**: Optional GZip compression for large cached objects
- ⚙️ **High-Performance Serialization**: MessagePack serialization with compression support
- 🔑 **Key Management**: Pattern-based key operations and prefix support
- 📊 **Connection Management**: Robust Redis connection handling with retry logic
- 🛡️ **Error Handling**: Comprehensive error handling and logging
- ⏱️ **TTL Support**: Time-to-live management and expiration control

## Installation

```bash
Install-Package RedisFlexCache
```

## Quick Start

### 1. Basic Setup

```csharp
using RedisFlexCache.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddRedisFlexCache("localhost:6379");
```

### 2. Using the Cache Service

```csharp
public class ProductService
{
    private readonly ICacheService _cache;

    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var cacheKey = $"product:{id}";
        
        // Try to get from cache first
        var cachedProduct = await _cache.GetAsync<Product>(cacheKey);
        if (cachedProduct != null)
        {
            return cachedProduct;
        }

        // If not in cache, get from database
        var product = await GetProductFromDatabaseAsync(id);
        
        // Cache for 30 minutes
        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(30));
        
        return product;
    }

    public async Task<Product> GetProductWithFactoryAsync(int id)
    {
        var cacheKey = $"product:{id}";
        
        // Get or set pattern - automatically handles cache miss
        return await _cache.GetAsync(cacheKey, 
            async () => await GetProductFromDatabaseAsync(id),
            TimeSpan.FromMinutes(30));
    }
}
```

## Configuration

### Using appsettings.json

```json
{
  "RedisCache": {
    "Connection": "localhost:6379",
    "KeyPrefix": "myapp",
    "DefaultExpiration": "00:30:00",
    "EnableCompression": true,
    "EnableHashKey": false,
    "ConnectionTimeout": 5000,
    "CommandTimeout": 5000,
    "ConnectionCount": 1,
    "MaxKeyLength": 100,
    "IsDbCachedActive": true,
    "Username": "",
    "Password": ""
  }
}
```

```csharp
// Register with configuration
builder.Services.AddRedisFlexCache(builder.Configuration);
```

### Programmatic Configuration

```csharp
builder.Services.AddRedisFlexCache(options =>
{
    options.Connection = "localhost:6379";
    options.KeyPrefix = "myapp";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
    options.EnableCompression = true;
    options.EnableHashKey = false;
    options.ConnectionTimeout = 5000;
    options.CommandTimeout = 5000;
    options.ConnectionCount = 1;
    options.MaxKeyLength = 100;
    options.IsDbCachedActive = true;
    options.Username = "";
    options.Password = "";
});
```

## Redis Cluster Support

RedisFlexCache provides built-in support for Redis clusters with automatic failover and load balancing. You can configure cluster connections using semicolon-separated connection strings.

### Cluster Configuration

#### Using Connection String Format

```csharp
// Multiple Redis nodes with semicolon separation
builder.Services.AddRedisFlexCache("127.0.0.1:6379;192.168.1.100:6379;192.168.1.101:6379");
```

#### Using appsettings.json

```json
{
  "RedisCache": {
    "Connection": "127.0.0.1:6379;192.168.1.100:6379;192.168.1.101:6379",
    "KeyPrefix": "myapp",
    "DefaultExpiration": "00:30:00",
    "EnableCompression": true,
    "ConnectionTimeout": 5000,
    "CommandTimeout": 5000
  }
}
```

#### Programmatic Configuration

```csharp
builder.Services.AddRedisFlexCache(options =>
{
    options.Connection = "127.0.0.1:6379;192.168.1.100:6379;192.168.1.101:6379";
    options.KeyPrefix = "myapp";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
    options.EnableCompression = true;
    options.ConnectionTimeout = 10000; // Increase timeout for cluster
    options.CommandTimeout = 10000;
});
```

### Cluster Connection String Format

The cluster connection string uses semicolon (`;`) as the delimiter between Redis nodes:

```
host1:port1;host2:port2;host3:port3;...
```

**Examples:**

```csharp
// Local cluster setup
"127.0.0.1:6379;127.0.0.1:6380;127.0.0.1:6381"

// Production cluster with different servers
"redis-node1.company.com:6379;redis-node2.company.com:6379;redis-node3.company.com:6379"

// Mixed IP and hostname configuration
"192.168.1.10:6379;redis-backup.local:6379;10.0.0.50:6379"

// Cluster with authentication
"user:password@redis1.example.com:6379;user:password@redis2.example.com:6379"
```

### Cluster Features

#### Automatic Failover
- **Node Detection**: Automatically detects and connects to available cluster nodes
- **Health Monitoring**: Continuously monitors node health and availability
- **Seamless Failover**: Automatically switches to healthy nodes when failures occur
- **Recovery**: Automatically reconnects to recovered nodes

#### Load Balancing
- **Distributed Operations**: Distributes cache operations across cluster nodes
- **Optimal Routing**: Routes commands to the appropriate node based on key hashing
- **Connection Pooling**: Maintains efficient connection pools to all cluster nodes

#### Cluster Topology
- **Dynamic Discovery**: Automatically discovers cluster topology and slot assignments
- **Slot Mapping**: Handles Redis cluster slot mapping transparently
- **Resharding Support**: Adapts to cluster resharding operations

### Best Practices for Cluster Configuration

#### 1. Connection Timeouts
```csharp
builder.Services.AddRedisFlexCache(options =>
{
    options.Connection = "node1:6379;node2:6379;node3:6379";
    options.ConnectionTimeout = 10000; // Increase for cluster latency
    options.CommandTimeout = 10000;    // Increase for cluster operations
});
```

#### 2. Minimum Node Configuration
```csharp
// Recommended: At least 3 nodes for proper cluster operation
var clusterNodes = "redis-master1:6379;redis-master2:6379;redis-master3:6379";
builder.Services.AddRedisFlexCache(clusterNodes);
```

#### 3. High Availability Setup
```csharp
// Include both master and replica nodes for maximum availability
var haCluster = "master1:6379;master2:6379;master3:6379;replica1:6379;replica2:6379;replica3:6379";
builder.Services.AddRedisFlexCache(haCluster);
```

#### 4. Network Considerations
```csharp
builder.Services.AddRedisFlexCache(options =>
{
    options.Connection = "10.0.1.10:6379;10.0.1.11:6379;10.0.1.12:6379";
    options.ConnectionTimeout = 15000; // Account for network latency
    options.CommandTimeout = 15000;
    options.ConnectionCount = 2;       // Multiple connections per node
});
```

### Cluster Monitoring and Diagnostics

#### Connection Status
```csharp
public class CacheHealthService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<CacheHealthService> _logger;

    public async Task<bool> CheckClusterHealthAsync()
    {
        try
        {
            // Test cluster connectivity
            var testKey = "health-check-" + Guid.NewGuid();
            await _cacheProvider.SetAsync(testKey, "test", TimeSpan.FromSeconds(10));
            var result = await _cacheProvider.GetAsync<string>(testKey);
            await _cacheProvider.RemoveAsync(testKey);
            
            return result == "test";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cluster health check failed");
            return false;
        }
    }
}
```

#### Logging Cluster Events
```csharp
// Enable detailed logging for cluster operations
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// RedisFlexCache will log:
// - Cluster node connections/disconnections
// - Failover events
// - Slot mapping changes
// - Performance metrics
```

### Cluster vs Single Node Comparison

| Feature | Single Node | Redis Cluster |
|---------|-------------|---------------|
| **Setup Complexity** | Simple | Moderate |
| **High Availability** | ❌ Single point of failure | ✅ Automatic failover |
| **Scalability** | ❌ Limited by single server | ✅ Horizontal scaling |
| **Performance** | ✅ Lower latency | ⚠️ Network overhead |
| **Data Distribution** | ❌ All data on one node | ✅ Distributed across nodes |
| **Maintenance** | ⚠️ Downtime required | ✅ Rolling updates possible |
| **Cost** | ✅ Lower infrastructure cost | ⚠️ Higher infrastructure cost |

### Migration from Single Node to Cluster

```csharp
// Before: Single node
builder.Services.AddRedisFlexCache("localhost:6379");

// After: Cluster configuration
builder.Services.AddRedisFlexCache("redis-node1:6379;redis-node2:6379;redis-node3:6379");

// No code changes required in your application!
// RedisFlexCache handles cluster operations transparently
```

**Migration Steps:**
1. Set up Redis cluster infrastructure
2. Update connection string to include all cluster nodes
3. Test cluster connectivity
4. Deploy application with new configuration
5. Monitor cluster performance and health

### Troubleshooting Cluster Issues

#### Common Issues and Solutions

**Connection Timeouts**
```csharp
// Increase timeouts for cluster latency
options.ConnectionTimeout = 15000;
options.CommandTimeout = 15000;
```

**Node Discovery Problems**
```csharp
// Ensure all cluster nodes are listed
options.Connection = "node1:6379;node2:6379;node3:6379;node4:6379;node5:6379;node6:6379";
```

**Network Connectivity**
```csharp
// Use IP addresses if DNS resolution is problematic
options.Connection = "192.168.1.10:6379;192.168.1.11:6379;192.168.1.12:6379";
```

**Slot Migration Errors**
```csharp
// Enable retry logic for slot migration scenarios
options.CommandTimeout = 20000; // Allow time for slot migrations
```

## Advanced Usage

### Key Operations

```csharp
// Remove cache entry
await _cache.RemoveAsync("product:123");

// Check if key exists
var exists = await _cache.ExistsAsync("product:123");

// Get remaining time to live
var ttl = await _cache.GetTimeToLiveAsync("product:123");

// Refresh expiration
var refreshed = await _cache.RefreshAsync("product:123", TimeSpan.FromHours(1));
```

### Batch Operations

```csharp
// Cache multiple products
var products = await GetProductsFromDatabaseAsync();
var tasks = products.Select(p => 
    _cache.SetAsync($"product:{p.Id}", p, TimeSpan.FromMinutes(30))
);
await Task.WhenAll(tasks);

// Get multiple products
var productIds = new[] { 1, 2, 3, 4, 5 };
var cachedProducts = new List<Product>();

foreach (var id in productIds)
{
    var product = await _cache.GetAsync<Product>($"product:{id}");
    if (product != null)
    {
        cachedProducts.Add(product);
    }
}
```

### Custom Cache Implementations

```csharp
public class CustomCacheService : RedisCacheService
{
    public CustomCacheService(IOptions<RedisCacheOptions> options, ILogger<CustomCacheService> logger)
        : base(options, logger)
    {
    }

    // Add custom methods or override existing ones
    public async Task<T> GetWithFallbackAsync<T>(string primaryKey, string fallbackKey)
    {
        var primary = await GetAsync<T>(primaryKey);
        if (primary != null) return primary;
        
        return await GetAsync<T>(fallbackKey);
    }
}

// Register custom implementation
builder.Services.AddRedisFlexCache<CustomCacheService>(options =>
{
    options.Connection = "localhost:6379";
});
```

### Service Lifetime Options

```csharp
// Singleton (default - recommended for most scenarios)
builder.Services.AddRedisFlexCache(options => { /* config */ });
builder.Services.AddRedisFlexCache(options => { /* config */ }, ServiceLifetime.Singleton);

// Scoped (per request in web applications)
builder.Services.AddRedisFlexCache(options => { /* config */ }, ServiceLifetime.Scoped);

// Transient (new instance every time)
builder.Services.AddRedisFlexCache(options => { /* config */ }, ServiceLifetime.Transient);

// With validation and custom lifetime
builder.Services.AddRedisFlexCacheWithValidation(options => { /* config */ }, validateOnStart: true, ServiceLifetime.Singleton);

// Legacy methods (still supported for backward compatibility)
builder.Services.AddRedisFlexCacheScoped(options => { /* config */ });
builder.Services.AddRedisFlexCacheTransient(options => { /* config */ });
```

### Choosing the Right Service Lifetime

**Singleton (Recommended)** 🏆
- ✅ Best performance - single shared instance
- ✅ Thread-safe Redis connections
- ✅ Optimal memory usage
- ✅ Recommended for most scenarios

**Scoped**
- ⚠️ New instance per HTTP request/scope
- ⚠️ Higher memory usage
- ⚠️ Use only if you need request-specific cache behavior

**Transient**
- ❌ New instance every time (not recommended)
- ❌ Poor performance and memory usage
- ❌ Potential connection exhaustion
- ❌ Avoid unless absolutely necessary

```csharp
// Recommended approach
builder.Services.AddRedisFlexCache(options => 
{
    options.Connection = "localhost:6379";
    options.KeyPrefix = "myapp";
}); // Defaults to Singleton
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Connection` | `string` | `"localhost:6379"` | Redis connection string |
| `KeyPrefix` | `string` | `null` | Prefix for all cache keys |
| `DefaultExpiration` | `TimeSpan` | `30 minutes` | Default expiration time |
| `EnableCompression` | `bool` | `false` | Enable compression for cached values |
| `EnableHashKey` | `bool` | `false` | Enable SHA256 hashing for cache keys |
| `ConnectionTimeout` | `int` | `5000` | Connection timeout (ms) |
| `CommandTimeout` | `int` | `5000` | Command timeout (ms) |
| `ConnectionCount` | `int` | `1` | Number of Redis connections |
| `MaxKeyLength` | `int` | `100` | Maximum cache key length |
| `IsDbCachedActive` | `bool` | `false` | Enable/disable caching |
| `Username` | `string` | `""` | Redis username for authentication |
| `Password` | `string` | `""` | Redis password for authentication |

## Thread Safety

RedisFlexCache is **fully thread-safe** and designed for concurrent access:

- ✅ **Connection Pool**: Uses multiple Redis connections with thread-safe connection pooling
- ✅ **Stateless Operations**: All cache operations are stateless and can be called concurrently
- ✅ **Singleton Registration**: Default singleton registration ensures thread-safe shared instances
- ✅ **StackExchange.Redis**: Built on top of the thread-safe StackExchange.Redis library
- ✅ **Concurrent Access**: Multiple threads can safely read/write to the cache simultaneously

```csharp
// Safe to use from multiple threads
var tasks = Enumerable.Range(1, 100).Select(async i => 
{
    await _cache.SetAsync($"key:{i}", $"value:{i}");
    return await _cache.GetAsync<string>($"key:{i}");
});

var results = await Task.WhenAll(tasks);
```

## Error Handling

The library includes comprehensive error handling:

```csharp
try
{
    var result = await _cache.GetAsync<Product>("product:123");
    // Handle result
}
catch (Exception ex)
{
    // Cache operations are designed to be non-blocking
    // Errors are logged but don't throw exceptions in most cases
    // You can still catch specific Redis exceptions if needed
}
```

## Logging

The library uses `Microsoft.Extensions.Logging` for comprehensive logging:

```csharp
// Configure logging in Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); // To see cache debug logs
```

Log levels:
- **Debug**: Cache hits/misses, key operations
- **Information**: Connection events, configuration
- **Warning**: Failed operations, retries
- **Error**: Connection failures, serialization errors

## Performance Tips

1. **Use Async Methods**: Always prefer async methods for better scalability
2. **Enable Compression**: For large objects, enable compression to reduce memory usage
3. **Set Appropriate TTL**: Use reasonable expiration times to balance performance and data freshness
4. **Use Key Prefixes**: Organize your cache keys with prefixes for easier management
5. **Batch Operations**: When possible, batch multiple cache operations
6. **Monitor Connection**: Keep an eye on Redis connection health in production
7. **Optimize Serialization**: For maximum performance, use MessagePack attributes on your models

### High-Performance Serialization

For optimal caching performance, decorate your models with MessagePack attributes:

```csharp
using MessagePack;

[MessagePackObject]
public class Product
{
    [Key(0)]
    public int Id { get; set; }
    
    [Key(1)]
    public string Name { get; set; }
    
    [Key(2)]
    public decimal Price { get; set; }
    
    [Key(3)]
    public DateTime CreatedAt { get; set; }
    
    [Key(4)]
    public List<string> Tags { get; set; }
}

[MessagePackObject]
public class User
{
    [Key(0)]
    public int UserId { get; set; }
    
    [Key(1)]
    public string Email { get; set; }
    
    [Key(2)]
    public string FirstName { get; set; }
    
    [Key(3)]
    public string LastName { get; set; }
    
    [Key(4)]
    public UserProfile Profile { get; set; }
}

[MessagePackObject]
public class UserProfile
{
    [Key(0)]
    public string Bio { get; set; }
    
    [Key(1)]
    public DateTime LastLoginAt { get; set; }
    
    [Key(2)]
    public Dictionary<string, object> Settings { get; set; }
}
```

**Benefits of MessagePack attributes:**
- **Faster Serialization**: Up to 10x faster than JSON serialization
- **Smaller Size**: Significantly reduced memory footprint
- **Type Safety**: Compile-time validation of serialization contracts
- **Version Tolerance**: Better handling of model evolution

**Best Practices:**
- Always use `[MessagePackObject]` on classes you plan to cache
- Assign sequential `[Key(n)]` attributes starting from 0
- Keep key numbers consistent across versions for backward compatibility
- Use meaningful key numbers that won't conflict with future properties

## Requirements

- .NET 8.0 or later
- Redis 3.0 or later

## Dependencies

- StackExchange.Redis (>= 2.7.20)
- Microsoft.Extensions.DependencyInjection.Abstractions (>= 9.0.8)
- Microsoft.Extensions.Configuration.Abstractions (>= 9.0.8)
- Microsoft.Extensions.Options (>= 9.0.8)
- Microsoft.Extensions.Options.ConfigurationExtensions (>= 9.0.8)
- Microsoft.Extensions.Logging.Abstractions (>= 9.0.8)
- MessagePack (>= 3.1.4)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.