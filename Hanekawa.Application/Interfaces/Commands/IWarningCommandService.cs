namespace Hanekawa.Application.Interfaces.Commands;

/// <summary>
/// Warning command service.
/// </summary>
public interface IWarningCommandService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task WarnUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason);
    /// <summary>
    /// Short summary of all users with valid warnings in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    Task<List<string>> Warns(ulong guildId);
    /// <summary>
    /// Gets all valid warnings for a user.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> WarnsAsync(ulong guildId, ulong userId);
    /// <summary>
    /// Clear warnings from a user, updating the validity of all to false
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <param name="moderatorId"></param>
    /// <param name="reason"></param>
    /// <param name="all"></param>
    /// <returns></returns>
    Task ClearUserWarnAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, bool all = false);
}