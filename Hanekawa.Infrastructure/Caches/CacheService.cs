using System;
using System.Text.Json;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Hanekawa.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Infrastructure.Caches;

internal class CacheService<T> : ICacheContext<T> where T : ICached
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService<T>> _logger;
    private readonly CacheKeyProvider<T> _cacheKey;

    public CacheService(IDistributedCache cache, ILogger<CacheService<T>> logger)
    {
        _cache = cache;
        _logger = logger;
        _cacheKey = new CacheKeyProvider<T>();
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key)
    {
        _logger.LogDebug("Retrieving cache value for key {Key}", key);
        var value = _cache.GetString(_cacheKey.GetKey(key));
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} not found", key);
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} found", key);
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key, TimeSpan expiration)
    {
        _logger.LogDebug("Retrieving cache value for key {Key}", key);
        var value = _cache.GetString(_cacheKey.GetKey(key));
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} not found", key);
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} found", key);
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value)
    {
        _logger.LogDebug("Adding cache value for key {Key}", key);
        var options = new DistributedCacheEntryOptions();
        _cache.SetString(_cacheKey.GetKey(key), JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value, TimeSpan expiration)
    {
        _logger.LogDebug("Adding cache value for key {Key}", key);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        _cache.SetString(_cacheKey.GetKey(key), JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        _logger.LogDebug("Removing cache value for key {Key}", key);
        _cache.Remove(_cacheKey.GetKey(key));
        return true;
    }

    /// <inheritdoc />
    public ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<TEntity> factory)
    {
        var value = Get<TEntity>(key);
        if (value is not null)
        {
            return new ValueTask<TEntity>(value);
        }

        var newValue = factory();
        Add(key, newValue);
        return new ValueTask<TEntity>(newValue);
    }
}