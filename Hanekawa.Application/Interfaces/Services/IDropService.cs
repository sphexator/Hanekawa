using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces.Services;

/// <summary>
/// Drop service
/// </summary>
public interface IDropService
{
    /// <summary>
    /// Initiate a drop event
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="user"></param>
    /// <param name="cancellationToken"><inheritdoc cref="CancellationToken"/></param>
    /// <returns></returns>
    Task DropAsync(TextChannel channel, DiscordMember user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Claims an active drop
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="msgId"></param>
    /// <param name="user"></param>
    /// <param name="cancellationToken"><inheritdoc cref="CancellationToken"/></param>
    /// <returns></returns>
    Task ClaimAsync(ulong channelId, ulong msgId, DiscordMember user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Configure drop config
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task Configure(Action<DropConfig> action);
}