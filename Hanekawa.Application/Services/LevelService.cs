using Hanekawa.Application.Contracts;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class LevelService : ILevelService
{
    private readonly IDbContext _db;
    private readonly ILogger<LevelService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public LevelService(IDbContext db, ILogger<LevelService> logger, IServiceProvider serviceProvider)
    {
        _db = db;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    /// <inheritdoc />
    public async Task<int?> AddExperience(DiscordMember member, int experience)
    {
        var config = await _db.GuildConfigs.Include(x => x.LevelConfig).ThenInclude(x => x.Rewards)
            .FirstOrDefaultAsync(x => x.GuildId == member.GuildId);
        if (config?.LevelConfig is null || !config.LevelConfig.LevelEnabled) return null;
        _logger.LogInformation("Adding {Experience} experience to guild user {User} in guild {Guild}", experience,
            member.UserId, member.GuildId);

        var user = await _db.Users.FindAsync(member.GuildId, member.UserId) 
                   ?? new GuildUser { GuildId = member.GuildId, UserId = member.UserId };
        var nextLevel = await _db.LevelRequirements.FindAsync(user.Level + 1);
        if (nextLevel is null)
        {
            _logger.LogInformation("User {User} in guild {Guild} has reached max level or {Level} is unreachable",
                member.UserId, member.GuildId, user.Level);
            return null;
        }
        
        if(user.Experience + experience >= nextLevel.Experience)
        {
            user.Level++;
            var result = await AdjustRoles(member, user.Level, config);
            _logger.LogInformation("User {User} in guild {Guild} has leveled up to level {Level}", member.UserId,
                member.GuildId, user.Level);
            await _serviceProvider.GetRequiredService<IMediator>().Send(new LevelUp
            {
                GuildId = member.GuildId,
                UserId = member.UserId,
                Level = user.Level,
                RoleIds = member.RoleIds
            });
        }

        user.Experience += experience;

        await _db.SaveChangesAsync();

        return experience;
    }
    /// <inheritdoc />
    public async Task<DiscordMember> AdjustRoles(DiscordMember member, int level, GuildConfig config)
    {
        foreach (var x in config.LevelConfig.Rewards)
        {
            if(!x.RoleId.HasValue) continue;
            if (x.Level <= level && !member.RoleIds.Contains(x.RoleId.Value)) member.RoleIds.Add(x.RoleId.Value);
            else if (x.Level > level && member.RoleIds.Contains(x.RoleId.Value)) member.RoleIds.Remove(x.RoleId.Value);
        }
        
        var bot = _serviceProvider.GetRequiredService<IBot>();
        await bot.ModifyRolesAsync(member, member.RoleIds.ToArray());
        return member;
    }
}