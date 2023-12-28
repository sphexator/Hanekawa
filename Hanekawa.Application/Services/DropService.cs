using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class DropService : IDropService
{
    private readonly ILevelService _levelService;
    private readonly ILogger<DropService> _logger;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random;
    
    public DropService(ILevelService levelService, ILogger<DropService> logger, IServiceProvider serviceProvider)
    {
        _levelService = levelService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _random = Random.Shared;
        _semaphoreSlim = new (1);
    }
    
    public DropService(ILevelService levelService, ILogger<DropService> logger, IServiceProvider serviceProvider, 
        Random random)
    {
        _levelService = levelService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _random = random;
        _semaphoreSlim = new (1);
    }
    
    /// <inheritdoc />
    public async Task DropAsync(TextChannel channel, DiscordMember user, CancellationToken cancellationToken = default)
    {
        var chance = _random.Next(1000);
        if (chance < 850) return;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var config = await db.GuildConfigs
            .Include(x => x.DropConfig)
            .FirstOrDefaultAsync(x => x.GuildId == channel.GuildId, 
                cancellationToken: cancellationToken);
        if (config is null || config.DropConfig.Blacklist.Contains(channel.Id)) return;

        var msg = await _serviceProvider.GetRequiredService<IBot>()
            .SendMessageAsync(channel.Id, "A drop event has been triggered!\n" +
                                           $"React with {config.DropConfig.Emote} as it appears to claim it!");

        var emotes = user.Guild.Emotes
            .OrderBy(e => _random.Next())
            .Take(3)
            .ToList();
        
        for (var i = 0; i < emotes.Count; i++)
        {
            Start:
            var emote = "123";
            await Task.Delay(1500, cancellationToken);
            if(emote is "") goto Start;
        }
        
        var cache = scope.ServiceProvider.GetRequiredService<ICacheContext>();
        cache.Add($"{msg.ChannelId}-{msg.Id}-drop", user.Id);
    }

    /// <inheritdoc />
    public async Task ClaimAsync(ulong channelId, ulong msgId, DiscordMember user, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        _logger.LogDebug("{UserId}-{GuildId} entered the semaphore for drop claims", 
            user.Id, user.Guild.Id);
        
        await using var scope = _serviceProvider.CreateAsyncScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheContext>();
        var value = cache.Get<int?>($"{msgId}-{channelId}-drop");
        if(value is null) return;

        var bot = _serviceProvider.GetRequiredService<IBot>();
        await bot.DeleteMessageAsync(user.Guild.Id, channelId, msgId);
        
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>(); 
        var config = await db.GuildConfigs
            .Include(x => x.DropConfig)
            .FirstOrDefaultAsync(x => x.GuildId == user.Guild.Id, 
                cancellationToken: cancellationToken);
        if (config is null) return;
        
        var exp = await _levelService.AddExperienceAsync(user, config.DropConfig.ExpReward);
        await bot.SendMessageAsync(channelId,
            $"Rewarded {user.Nickname ?? user.Username} with {exp ?? 0} experience for claiming the drop!");
        
        cache.Remove($"{msgId}-{channelId}-drop");
        _semaphoreSlim.Release();
        _logger.LogDebug("{UserId}-{GuildId} exited the semaphore for drop claims", 
            user.Id, user.Guild.Id);
    }
    
    /// <inheritdoc />
    public async Task Configure(Action<DropConfig> action)
    {
        var config = new DropConfig();
        action.Invoke(config);
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var cfg = await db.GuildConfigs
            .Include(x => x.DropConfig)
            .FirstOrDefaultAsync(x => x.GuildId == config.GuildId) 
                  ?? new GuildConfig { GuildId = config.GuildId };

        if (cfg.DropConfig is null)
        {
            cfg.DropConfig = config;
            goto Save;
        }
        
        if (config.ExpReward is not 100) cfg.DropConfig.ExpReward = config.ExpReward;
        if (config.Emote is not "") cfg.DropConfig.Emote = config.Emote;
        
        if (config.Blacklist.Length > 0)
        {
            var newBlacklist = new ulong[cfg.DropConfig.Blacklist.Length + config.Blacklist.Length];
            for (var i = 0; i < cfg.DropConfig.Blacklist.Length + config.Blacklist.Length; i++)
            {
                var x = i >= cfg.DropConfig.Blacklist.Length 
                    ? config.Blacklist[i - cfg.DropConfig.Blacklist.Length] 
                    : cfg.DropConfig.Blacklist[i];
                newBlacklist[i] = x;
            }
        }
        Save:
        await db.SaveChangesAsync();
    }
}