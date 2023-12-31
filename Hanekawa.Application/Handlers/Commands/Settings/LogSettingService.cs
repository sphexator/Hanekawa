using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Commands.Settings;

public class LogSettingService : ILogService
{
    private readonly ILogger<LogSettingService> _logger;
    private readonly IDbContext _dbContext;

    public LogSettingService(ILogger<LogSettingService> logger, IDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SetJoinLeaveLogChannelAsync(ulong guildId, ulong? channelId)
    {
        var cfg = await _dbContext.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        if (cfg?.LogConfig is null) return;
        cfg.LogConfig.JoinLeaveLogChannelId = channelId;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Set join/leave log channel to {Channel} for guild {Guild}", channelId, guildId);
    }

    public async Task SetMessageLogChannelAsync(ulong guildId, ulong? channelId)
    {
        var cfg = await _dbContext.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        
        if (cfg?.LogConfig is null) return;
        cfg.LogConfig.MessageLogChannelId = channelId;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Set message log channel to {Channel} for guild {Guild}", channelId, guildId);
    }

    public async Task SetModLogChannelAsync(ulong guildId, ulong? channelId)
    {
        var cfg = await _dbContext.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        if (cfg?.LogConfig is null) return;
        cfg.LogConfig.ModLogChannelId = channelId;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Set mod log channel to {Channel} for guild {Guild}", channelId, guildId);
    }

    public async Task SetVoiceLogChannelAsync(ulong guildId, ulong? channelId)
    {
        var cfg = await _dbContext.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        if (cfg?.LogConfig is null) return;
        cfg.LogConfig.VoiceLogChannelId = channelId;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Set voice log channel to {Channel} for guild {Guild}", channelId, guildId);
    }
}