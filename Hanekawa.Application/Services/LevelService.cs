using Hanekawa.Application.Contracts;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class LevelService : ILevelService
{
    private readonly IDbContext _db;
    private readonly IBot _bot;
    private readonly ILogger<LevelService> _logger;
    private readonly IRequestDispatcher _dispatcher;

    public LevelService(IDbContext db, ILogger<LevelService> logger,
        IBot bot, IRequestDispatcher dispatcher)
    {
        _db = db;
        _logger = logger;
        _bot = bot;
        _dispatcher = dispatcher;
    }

    /// <inheritdoc />
    public async Task<int?> AddExperienceAsync(DiscordMember member, int experience)
    {
        var config = await _db.GuildConfigs.Include(x => x.LevelConfig)
            .ThenInclude(x => x.Rewards)
            .FirstOrDefaultAsync(x => x.GuildId == member.Guild.GuildId);
        if (config?.LevelConfig is null || !config.LevelConfig.LevelEnabled) return null;
        _logger.LogInformation("Adding {Experience} experience to guild user {User} in guild {Guild}",
            experience, member.Id, member.Guild.GuildId);

        var user = await _db.GetOrCreateUserAsync(member.Guild.GuildId, member.Id);
        var nextLevel = await _db.LevelRequirements.FirstOrDefaultAsync(x => x.Level == user.Level + 1);
        if(nextLevel is not null && user.Experience + experience >= nextLevel.Experience)
        {
            user.Level++;
            await AdjustRolesAsync(member, user.Level, config);
            _logger.LogInformation("User {User} in guild {Guild} has leveled up to level {Level}",
                member.Id, member.Guild.GuildId, user.Level);
            await _dispatcher.SendAsync(new LevelUp(member, member.RoleIds, user.Level, config));
        }

        user.Experience += experience;
        await _db.SaveChangesAsync();

        return experience;
    }
    /// <inheritdoc />
    public async Task<DiscordMember> AdjustRolesAsync(DiscordMember member, int level, GuildConfig config)
    {
        for (var i = 0; i < config.LevelConfig?.Rewards.Count; i++)
        {
            AdjustRoles(member, level, config.LevelConfig.Rewards[i]);
        }

        await _bot.ModifyRolesAsync(member, member.RoleIds);
        return member;
    }
    private static void AdjustRoles(DiscordMember member, int level, LevelReward x)
    {
        if (!x.RoleId.HasValue)
        {
            return;
        }
        if (x.Level <= level && !member.RoleIds.Contains(x.RoleId.Value))
        {
            member.RoleIds = member.RoleIds.Add(x.RoleId.Value);
        }
        else if (x.Level > level && member.RoleIds.Contains(x.RoleId.Value))
        {
            member.RoleIds = member.RoleIds.Remove(x.RoleId.Value);
        }
    }
}
