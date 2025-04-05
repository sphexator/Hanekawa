using System;
using System.Text.Json;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Hanekawa.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Infrastructure.Caches;

internal class CacheService : ICacheContext
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key) where TEntity : ICached
    {
        _logger.LogDebug("Retrieving cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var cacheKey = new CacheKeyProvider<TEntity>();
        var value = _cache.GetString(cacheKey.GetKey(key));
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} of type {CacheType} not found", key, nameof(TEntity));
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} of type {CacheType} found", key, nameof(TEntity));
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key, TimeSpan expiration) where TEntity : ICached
    {
        _logger.LogDebug("Retrieving cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var cacheKey = new CacheKeyProvider<TEntity>();
        var value = _cache.GetString(cacheKey.GetKey(key));
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} of type {CacheType} not found", key, nameof(TEntity));
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} of type {CacheType} found", key, nameof(TEntity));
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value) where TEntity : ICached
    {
        _logger.LogDebug("Adding cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var cacheKey = new CacheKeyProvider<TEntity>();
        var options = new DistributedCacheEntryOptions();
        _cache.SetString(cacheKey.GetKey(key), JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value, TimeSpan expiration) where TEntity : ICached
    {
        _logger.LogDebug("Adding cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var cacheKey = new CacheKeyProvider<TEntity>();
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        _cache.SetString(cacheKey.GetKey(key), JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public bool Remove<TEntity>(string key) where TEntity : ICached
    {
        _logger.LogDebug("Removing cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var cacheKey = new CacheKeyProvider<TEntity>();
        _cache.Remove(cacheKey.GetKey(key));
        return true;
    }

    /// <inheritdoc />
    public async ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<Task<TEntity>> factory) where TEntity : ICached
    {
        var value = Get<TEntity>(key);
        if (value is not null)
        {
            return await new ValueTask<TEntity>(value);
        }

        var newValue = await factory();
        Add(key, newValue);
        return await new ValueTask<TEntity>(newValue);
    }
}