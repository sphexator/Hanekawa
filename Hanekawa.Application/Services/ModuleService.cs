using System.Text.Json;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Hanekawa.Application.Services;

public class ModuleService : IModuleService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    private readonly IDistributedCache _cache;
    private readonly IDbContext _db;

    public ModuleService(IDistributedCache cache, IDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsEnabledAsync(ulong guildId, string module,
        CancellationToken cancellationToken = default)
    {
        var states = await GetStatesAsync(guildId, cancellationToken);
        return states.GetValueOrDefault(module, false);
    }

    /// <inheritdoc />
    public async Task SetEnabledAsync(ulong guildId, string module, bool enabled,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Modules
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Name == module, cancellationToken);
        if (entity is null)
        {
            entity = new Module { GuildId = guildId, Name = module, Enabled = enabled };
            await _db.Modules.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.Enabled = enabled;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var states = await GetStatesAsync(guildId, cancellationToken);
        states[module] = enabled;
        await SetCacheAsync(guildId, states, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Module>> GetModulesAsync(ulong guildId,
        CancellationToken cancellationToken = default)
    {
        var states = await GetStatesAsync(guildId, cancellationToken);
        var result = new List<Module>(ModuleName.All.Length);
        foreach (var name in ModuleName.All)
        {
            result.Add(new Module
            {
                GuildId = guildId,
                Name = name,
                Enabled = states.GetValueOrDefault(name, false)
            });
        }
        return result;
    }

    private async Task<Dictionary<string, bool>> GetStatesAsync(ulong guildId,
        CancellationToken cancellationToken)
    {
        var cached = await _cache.GetStringAsync(KeyName(guildId), cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            var states = JsonSerializer.Deserialize<Dictionary<string, bool>>(cached);
            if (states is not null) return states;
        }

        var dbStates = await _db.Modules
            .Where(x => x.GuildId == guildId)
            .Select(x => new { x.Name, x.Enabled })
            .ToListAsync(cancellationToken);
        var result = dbStates.ToDictionary(x => x.Name, x => x.Enabled);
        await SetCacheAsync(guildId, result, cancellationToken);
        return result;
    }

    private Task SetCacheAsync(ulong guildId, Dictionary<string, bool> states,
        CancellationToken cancellationToken)
        => _cache.SetStringAsync(KeyName(guildId), JsonSerializer.Serialize(states), CacheOptions,
            cancellationToken);

    private static string KeyName(ulong guildId) => $"{guildId}-Modules";
}
