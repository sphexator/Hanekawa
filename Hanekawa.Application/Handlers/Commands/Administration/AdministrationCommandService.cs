using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Commands.Administration;

/// <inheritdoc />
public class AdministrationCommandService(IBot bot, ILogger<AdministrationCommandService> logger) 
    : IAdministrationCommandService
{
    /// <inheritdoc />
    public async Task<Response<Message>> BanUserAsync(DiscordMember user, ulong moderatorId, string reason, 
        int days = 0)
    {
        logger.LogInformation("Banning user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await bot.BanAsync(user.Guild.Id, user.Id, days, reason).ConfigureAwait(false);
        return new (new (
            string.Format(Localization.BannedGuildUser, user.Mention, user.Guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> UnbanUserAsync(Guild guild, ulong userId, ulong moderatorId, string reason)
    {
        logger.LogInformation("Unbanning user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            userId, guild.Id, moderatorId, reason);
        await bot.UnbanAsync(guild.Id, userId, reason).ConfigureAwait(false);
        return new (new (
            string.Format(Localization.UnbannedGuildUser, userId, guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> KickUserAsync(DiscordMember user, ulong moderatorId, string reason)
    {
        logger.LogInformation("Kicking user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await bot.KickAsync(user.Guild.Id, user.Id, reason).ConfigureAwait(false);
        return new (new (
            string.Format(Localization.KickedGuildUser, user.Username, user.Guild.Name)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> MuteUserAsync(DiscordMember user, ulong moderatorId, string reason, 
        TimeSpan duration)
    {
        logger.LogInformation("Muting user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason} for duration {Duration}", 
            user.Id, user.Guild.Id, moderatorId, reason, duration);
        await bot.MuteAsync(user.Guild.Id, user.Id, reason, duration).ConfigureAwait(false);
        return new (new (string.Format(Localization.MutedGuildUserDuration, 
            user.Mention, duration.Humanize())));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> UnmuteUserAsync(DiscordMember user, ulong moderatorId, string reason)
    {
        logger.LogInformation("Unmuting user {UserId} from guild {GuildId} by moderator {ModeratorId} for " +
                               "reason {Reason}", 
            user.Id, user.Guild.Id, moderatorId, reason);
        await bot.UnmuteAsync(user.Guild.Id, user.Id, reason).ConfigureAwait(false);
        return new (new (string.Format(Localization.UnMutedUser, user.Mention)));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> AddRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId)
    {
        logger.LogInformation("Adding role {RoleId} to user {UserId} from guild {GuildId} by moderator " +
                               "{ModeratorId}", 
            roleId, user.Id, user.Guild.Id, moderatorId);
        await bot.AddRoleAsync(user.Guild.Id, user.Id, roleId).ConfigureAwait(false);
        return new (new (""));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> RemoveRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId)
    {
        logger.LogInformation("Removing role {RoleId} from user {UserId} from guild {GuildId} by moderator " +
                               "{ModeratorId}", 
            roleId, user.Id, user.Guild.Id, moderatorId);
        await bot.RemoveRoleAsync(user.Guild.Id, user.Id, roleId).ConfigureAwait(false);
        return new (new (""));
    }
    
    /// <inheritdoc />
    public async Task<Response<Message>> PruneAsync(ulong guildId, ulong channelId, ulong[] messageIds, 
        ulong moderatorId, string reason)
    {
        logger.LogInformation("Pruning {MessageAmount} messages from channel {ChannelId} by moderator " +
                               "{ModeratorId} for reason {Reason}", 
            messageIds.Length, channelId, moderatorId, reason);
        await bot.PruneMessagesAsync(guildId, channelId, messageIds).ConfigureAwait(false);
        return new (new (string.Format(Localization.PrunedMessages, messageIds.Length)));
    }
}