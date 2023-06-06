using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IBot
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="days"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Task BanAsync(ulong guildId, ulong userId, int days, string reason);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Task UnbanAsync(ulong guildId, ulong userId, string reason);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Task KickAsync(ulong guildId, ulong userId, string reason);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="reason"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public Task MuteAsync(ulong guildId, ulong userId, string reason, TimeSpan duration);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Task UnmuteAsync(ulong guildId, ulong userId, string reason); 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="member"></param>
    /// <param name="modifiedRoles"></param>
    /// <returns></returns>
    public Task ModifyRolesAsync(DiscordMember member, ulong[] modifiedRoles);
    /// <summary>
    /// Gets a channel from a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    public ulong? GetChannel(ulong guildId, ulong channelId);
    /// <summary>
    /// Prune messages from a channel
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <param name="messageIds"></param>
    /// <returns></returns>
    public Task PruneMessagesAsync(ulong guildId, ulong channelId, ulong[] messageIds);

    /// <summary>
    /// Sends a message to a channel
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="message"></param>
    /// <param name="attachment"></param>
    /// <returns></returns>
    public Task SendMessageAsync(ulong channelId, string message, Attachment? attachment = null);

    /// <summary>
    /// Sends a message to a channel with an embed
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="embedMessage"></param>
    /// <param name="attachment"></param>
    /// <returns></returns>
    public Task SendMessageAsync(ulong channelId, Embed embedMessage, Attachment? attachment = null);
}