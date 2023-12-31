using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Commands.Administration;

/// <inheritdoc />
public class AdministrationCommandService : IAdministrationCommandService
{
    private readonly IBot _bot;
    private readonly ILogger<AdministrationCommandService> _logger;
    public AdministrationCommandService(IBot bot, ILogger<AdministrationCommandService> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Response<Message>> BanUserAsync(DiscordMember user, ulong moderatorId, string reason, 
        int days = 0)
    {
        _logger.LogInformation("Banning user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await _bot.BanAsync(user.Guild.Id, user.Id, days, reason);
        return new (new (
            string.Format(Localization.BannedGuildUser, user.Mention, user.Guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> UnbanUserAsync(Guild guild, ulong userId, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Unbanning user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            userId, guild.Id, moderatorId, reason);
        await _bot.UnbanAsync(guild.Id, userId, reason);
        return new (new (
            string.Format(Localization.UnbannedGuildUser, userId, guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> KickUserAsync(DiscordMember user, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Kicking user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await _bot.KickAsync(user.Guild.Id, user.Id, reason);
        return new (new (
            string.Format(Localization.KickedGuildUser, user.Username, user.Guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> MuteUserAsync(DiscordMember user, ulong moderatorId, string reason, 
        TimeSpan duration)
    {
        _logger.LogInformation("Muting user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason} for duration {Duration}", 
            user.Id, user.Guild.Id, moderatorId, reason, duration);
        await _bot.MuteAsync(user.Guild.Id, user.Id, reason, duration);
        return new (new (string.Format(Localization.MutedGuildUserDuration, 
            user.Mention, duration.Humanize())));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> UnmuteUserAsync(DiscordMember user, ulong moderatorId, string reason)
    {
        _logger.LogInformation("Unmuting user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await _bot.UnmuteAsync(user.Guild.Id, user.Id, reason);
        return new (new (string.Format(Localization.UnMutedUser, user.Mention)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> AddRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId)
    {
        _logger.LogInformation("Adding role {RoleId} to user {UserId} from guild {GuildId} by moderator " +
                               "{ModeratorId}", 
            roleId, user.Id, user.Guild.Id, moderatorId);
        await _bot.AddRoleAsync(user.Guild.Id, user.Id, roleId);
        return new (new (""));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> RemoveRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId} from guild {GuildId} by moderator " +
                               "{ModeratorId}", 
            roleId, user.Id, user.Guild.Id, moderatorId);
        await _bot.RemoveRoleAsync(user.Guild.Id, user.Id, roleId);
        return new (new (""));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> PruneAsync(ulong guildId, ulong channelId, ulong[] messageIds, 
        ulong moderatorId, string reason)
    {
        _logger.LogInformation("Pruning {MessageAmount} messages from channel {ChannelId} by moderator " +
                               "{ModeratorId} for reason {Reason}", 
            messageIds.Length, channelId, moderatorId, reason);
        await _bot.PruneMessagesAsync(guildId, channelId, messageIds);
        return new (new (string.Format(Localization.PrunedMessages, messageIds.Length)));
    }
}