namespace Hanekawa.Application.Interfaces.Commands;

public interface IAdministrationCommandService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    Task BanUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, int days = 0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task UnbanUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task KickUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task MuteUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, TimeSpan duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task UnmuteUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task AddRoleAsync(ulong guildId, ulong userId, ulong moderatorId, ulong roleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task RemoveRoleAsync(ulong guildId, ulong userId, ulong moderatorId, ulong roleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moderatorId"></param>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <param name="messageIds"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task PruneAsync(ulong guildId, ulong channelId, ulong[] messageIds, ulong moderatorId, string reason);
}