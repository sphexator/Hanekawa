using Hanekawa.Application.Handlers.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Commands.Administration;

/// <inheritdoc />
public class AdministrationCommandService : IAdministrationCommandService
{
    private readonly IBot _bot;
    private readonly ILogger<AdministrationCommandService> _logger;
    private readonly IServiceProvider _serviceProvider;
    public AdministrationCommandService(IBot bot, ILogger<AdministrationCommandService> logger, IServiceProvider serviceProvider)
    {
        _bot = bot;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task BanUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, int days = 0)
    {
        _logger.LogInformation("Banning user {UserId} from guild {GuildId} by moderator {ModeratorId} for reason {Reason}", 
            userId, guildId, moderatorId, reason);
        await _bot.BanAsync(guildId, userId, days, reason);
    }
    /// <inheritdoc />
    public async Task UnbanUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Unbanning user {UserId} from guild {GuildId} by moderator {ModeratorId} for reason {Reason}", 
            userId, guildId, moderatorId, reason);
        await _bot.UnbanAsync(guildId, userId, reason);
    }
    /// <inheritdoc />
    public async Task KickUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Kicking user {UserId} from guild {GuildId} by moderator {ModeratorId} for reason {Reason}", 
            userId, guildId, moderatorId, reason);
        await _bot.KickAsync(guildId, userId, reason);
    }
    /// <inheritdoc />
    public async Task MuteUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, TimeSpan duration)
    {
        _logger.LogInformation("Muting user {UserId} from guild {GuildId} by moderator {ModeratorId} for reason {Reason} for duration {Duration}", 
            userId, guildId, moderatorId, reason, duration);
        await _bot.MuteAsync(guildId, userId, reason, duration);
    }
    /// <inheritdoc />
    public async Task UnmuteUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Unmuting user {UserId} from guild {GuildId} by moderator {ModeratorId} for reason {Reason}", 
            userId, guildId, moderatorId, reason);
        await _bot.UnmuteAsync(guildId, userId, reason);
    }
    /// <inheritdoc />
    public async Task AddRoleAsync(ulong guildId, ulong userId, ulong moderatorId, ulong roleId)
    {
        _logger.LogInformation("Adding role {RoleId} to user {UserId} from guild {GuildId} by moderator {ModeratorId}", 
            roleId, userId, guildId, moderatorId);
        await _bot.AddRoleAsync(guildId, userId, roleId);
    }
    /// <inheritdoc />
    public async Task RemoveRoleAsync(ulong guildId, ulong userId, ulong moderatorId, ulong roleId)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId} from guild {GuildId} by moderator {ModeratorId}", 
            roleId, userId, guildId, moderatorId);
        await _bot.RemoveRoleAsync(guildId, userId, roleId);
    }
    /// <inheritdoc />
    public async Task PruneAsync(ulong guildId, ulong channelId, ulong[] messageIds, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Pruning {MessageAmount} messages from channel {ChannelId} by moderator {ModeratorId} for reason {Reason}", 
            messageIds.Length, channelId, moderatorId, reason);
        await _bot.PruneMessagesAsync(guildId, channelId, messageIds);
    }
}