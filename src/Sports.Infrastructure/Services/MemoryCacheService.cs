using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sports.Application.Services;

namespace Sports.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private static readonly HashSet<string> _cacheKeys = new();
    private static readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                lock (_lock)
                {
                    _cacheKeys.Remove(k.ToString()!);
                }
                _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", k, r);
            });

            _cache.Set(key, value, options);

            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Cache SET for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            
            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }

            _logger.LogDebug("Cache REMOVED for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            List<string> keysToRemove;
            
            lock (_lock)
            {
                keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            lock (_lock)
            {
                foreach (var key in keysToRemove)
                {
                    _cacheKeys.Remove(key);
                }
            }

            _logger.LogDebug("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }

        return Task.CompletedTask;
    }
}
