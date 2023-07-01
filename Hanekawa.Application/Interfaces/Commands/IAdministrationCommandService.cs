using Hanekawa.Entities;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces.Commands;

public interface IAdministrationCommandService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    Task<Response<Message>> BanUserAsync(DiscordMember user, ulong moderatorId, string reason, int days = 0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guild"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task<Response<Message>> UnbanUserAsync(Guild guild, ulong userId, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task<Response<Message>> KickUserAsync(DiscordMember user, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task<Response<Message>> MuteUserAsync(DiscordMember user, ulong moderatorId, string reason, TimeSpan duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task<Response<Message>> UnmuteUserAsync(DiscordMember user, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task<Response<Message>> AddRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="moderatorId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task<Response<Message>> RemoveRoleAsync(DiscordMember user, ulong moderatorId, ulong roleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moderatorId"></param>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <param name="messageIds"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task<Response<Message>> PruneAsync(ulong guildId, ulong channelId, ulong[] messageIds, ulong moderatorId, string reason);
}