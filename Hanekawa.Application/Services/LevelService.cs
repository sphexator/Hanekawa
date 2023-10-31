using Hanekawa.Application.Contracts;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class LevelService : ILevelService
{
    private readonly IDbContext _db;
    private readonly IBot _bot;
    private readonly ILogger<LevelService> _logger;
    private readonly IMediator _mediator;
    
    public LevelService(IDbContext db, ILogger<LevelService> logger, 
        IBot bot, IMediator mediator)
    {
        _db = db;
        _logger = logger;
        _bot = bot;
        _mediator = mediator;
    }
    
    /// <inheritdoc />
    public async Task<int?> AddExperienceAsync(DiscordMember member, int experience)
    {
        var config = await _db.GuildConfigs.Include(x => x.LevelConfig)
            .ThenInclude(x => x.Rewards)
            .FirstOrDefaultAsync(x => x.GuildId == member.Guild.Id);
        if (config?.LevelConfig is null || !config.LevelConfig.LevelEnabled) return null;
        _logger.LogInformation("Adding {Experience} experience to guild user {User} in guild {Guild}", 
            experience, member.Id, member.Guild.Id);

        var user = await _db.Users.FindAsync(member.Guild.Id, member.Id) 
                   ?? new GuildUser { GuildId = member.Guild.Id, UserId = member.Id };
        var nextLevel = await _db.LevelRequirements.FindAsync(user.Level + 1);
        if(nextLevel is not null && user.Experience + experience >= nextLevel.Experience)
        {
            user.Level++;
            await AdjustRolesAsync(member, user.Level, config);
            _logger.LogInformation("User {User} in guild {Guild} has leveled up to level {Level}", 
                member.Id, member.Guild.Id, user.Level);
            await _mediator.Send(new LevelUp(member, member.RoleIds, user.Level, config));
        }

        user.Experience += experience;
        await _db.SaveChangesAsync();

        return experience;
    }
    
    /// <inheritdoc />
    public async Task<DiscordMember> AdjustRolesAsync(DiscordMember member, int level, GuildConfig config)
    {
        for (var i = 0; i < config.LevelConfig.Rewards.Count; i++)
        {
            var x = config.LevelConfig.Rewards[i];
            if (!x.RoleId.HasValue) continue;
            if (x.Level <= level && !member.RoleIds.Contains(x.RoleId.Value)) member.RoleIds.Add(x.RoleId.Value);
            else if (x.Level > level && member.RoleIds.Contains(x.RoleId.Value)) member.RoleIds.Remove(x.RoleId.Value);
        }
        
        await _bot.ModifyRolesAsync(member, member.RoleIds.ToArray());
        return member;
    }
}