namespace Hanekawa.Application.Interfaces.Commands;

/// <summary>
/// 
/// </summary>
public interface ILogService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    Task SetJoinLeaveLogChannelAsync(ulong guildId, ulong? channelId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    Task SetMessageLogChannelAsync(ulong guildId, ulong? channelId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    Task SetModLogChannelAsync(ulong guildId, ulong? channelId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    Task SetVoiceLogChannelAsync(ulong guildId, ulong? channelId);
}