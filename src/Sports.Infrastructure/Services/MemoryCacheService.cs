using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sports.Application.Services;

namespace Sports.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache Cache;
    private readonly ILogger<MemoryCacheService> Logger;
    private static readonly HashSet<string> CacheKeys = new();
    private static readonly object Lock = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        Cache = cache;
        Logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (Cache.TryGetValue(key, out T? value))
            {
                Logger.LogDebug("Cache HIT for key: {Key}", key);
                return Task.FromResult(value);
            }

            Logger.LogDebug("Cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving from cache: {Key}", key);
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
                lock (Lock)
                {
                    CacheKeys.Remove(k.ToString()!);
                }
                Logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", k, r);
            });

            Cache.Set(key, value, options);

            lock (Lock)
            {
                CacheKeys.Add(key);
            }

            Logger.LogDebug("Cache SET for key: {Key}", key);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting cache: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            Cache.Remove(key);
            
            lock (Lock)
            {
                CacheKeys.Remove(key);
            }

            Logger.LogDebug("Cache REMOVED for key: {Key}", key);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing from cache: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            List<string> keysToRemove;
            
            lock (Lock)
            {
                keysToRemove = CacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                Cache.Remove(key);
            }

            lock (Lock)
            {
                foreach (var key in keysToRemove)
                {
                    CacheKeys.Remove(key);
                }
            }

            Logger.LogDebug("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }

        return Task.CompletedTask;
    }
}
