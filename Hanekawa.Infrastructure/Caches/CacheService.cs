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
    public TEntity? Get<TEntity>(string key)
    {
        _logger.LogDebug("Retrieving cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var value = _cache.GetString(key);
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} of type {CacheType} not found", key, nameof(TEntity));
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} of type {CacheType} found", key, nameof(TEntity));
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key, TimeSpan expiration)
    {
        _logger.LogDebug("Retrieving cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var value = _cache.GetString(key);
        if (value is null)
        {
            _logger.LogDebug("Cache value for key {Key} of type {CacheType} not found", key, nameof(TEntity));
            return default;
        }

        _logger.LogDebug("Cache value for key {Key} of type {CacheType} found", key, nameof(TEntity));
        return JsonSerializer.Deserialize<TEntity>(value);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value)
    {
        _logger.LogDebug("Adding cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var options = new DistributedCacheEntryOptions();
        _cache.SetString(key, JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public void Add<TEntity>(string key, TEntity value, TimeSpan expiration)
    {
        _logger.LogDebug("Adding cache value for key {Key} of type {CacheType}", key, nameof(TEntity));
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        _cache.SetString(key, JsonSerializer.Serialize(value), options);
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        _logger.LogDebug("Removing cache value for key {Key}", key);
        _cache.Remove(key);
        return true;
    }

    /// <inheritdoc />
    public async ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<Task<TEntity>> factory)
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