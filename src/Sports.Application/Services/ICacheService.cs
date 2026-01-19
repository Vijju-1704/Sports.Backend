namespace Sports.Application.Services;

/// <summary>
/// Interface for caching operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached item by key
    /// </summary>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Set a cached item with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Remove a cached item by key
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Remove all cached items that start with a given prefix
    /// </summary>
    Task RemoveByPrefixAsync(string prefix);
}
