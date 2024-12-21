using System.Linq.Expressions;
using System.Text.Json;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Hanekawa.Application.Services;

public class ConfigService : IConfigService
{
    private readonly IDistributedCache _cache;
    private readonly IDbContext _db;

    public ConfigService(IDistributedCache cache, IDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public async ValueTask<GuildConfig> GetAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(KeyName<GuildConfig>(guildId), cancellationToken);
        if (value is not null or { Length: 0 })
        {
            var cachedConfig = JsonSerializer.Deserialize<GuildConfig>(value);
            if (cachedConfig is not null)
            {
                return cachedConfig;
            }
        }
        var config = await _db.GetOrCreateConfigAsync(guildId, cancellationToken);

        await SetAsync(guildId, config, cancellationToken);
        return config;
    }

    public async ValueTask<GuildConfig> GetAsync(ulong guildId, Type include,
        CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(KeyName<GuildConfig>(guildId), cancellationToken);
        if (value is not null or { Length: 0 })
        {
            var cachedConfig = JsonSerializer.Deserialize<GuildConfig>(value);
            var includeValue = cachedConfig?.GetType()
                .GetProperties()
                .FirstOrDefault(x => nameof(x.PropertyType) == include.Name)
                ?.GetValue(cachedConfig);
            if(cachedConfig is not null && includeValue is not null)
            {
                return cachedConfig;
            }
        }

        var config = await _db.GetOrCreateConfigAsync(guildId, include, cancellationToken);
        await SetAsync(guildId, config, cancellationToken);
        return config;
    }

    public async ValueTask SetAsync<T>(ulong key, T value, CancellationToken cancellationToken = default)
        where T : class, IConfig, new()
    {
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(KeyName<GuildConfig>(key), json,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) }, token: cancellationToken);
    }

    public async ValueTask RemoveAsync<T>(ulong guildId, CancellationToken cancellationToken = default)
        where T : class, IConfig, new()
    {
        await _cache.RemoveAsync(KeyName<GuildConfig>(guildId), cancellationToken);
    }

    private static string KeyName<T>(ulong key) where T : class, IConfig
        => $"{key}-{typeof(T).Name}";
}

public interface IConfigService
{
    ValueTask<GuildConfig> GetAsync(ulong guildId, CancellationToken cancellationToken = default);
    ValueTask<GuildConfig> GetAsync(ulong guildId, Type include,
        CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(ulong key, T value, CancellationToken cancellationToken = default) where T : class, IConfig, new();
    ValueTask RemoveAsync<T>(ulong guildId, CancellationToken cancellationToken = default) where T : class, IConfig, new();
}