using Hanekawa.Entities.Activity;

namespace Hanekawa.Application.Interfaces.Services;

public interface IActivityService
{
    /// <summary>
    /// Counts a message towards the user's weekly activity.
    /// </summary>
    Task TrackMessageAsync(ulong guildId, ulong userId, DateTimeOffset timestamp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most active users for the current week, ordered by message count.
    /// </summary>
    Task<IReadOnlyList<GuildActivity>> GetWeeklyLeaderboardAsync(ulong guildId, int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs the current week's most-active role to the configured amount of top users,
    /// removing it from users that have been overtaken.
    /// </summary>
    Task SyncCurrentWeekRolesAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes week rollovers for all guilds: hands out the previous week's role reward
    /// to its most active user, resets current-week role holders and posts the configured
    /// weekly announcement.
    /// </summary>
    Task ProcessWeekRolloversAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the role rewarded to the previous week's most active user. Null disables it.
    /// </summary>
    Task<string> SetPreviousWeekRoleAsync(ulong guildId, ulong? roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the role held by the current week's most active users and how many top users hold it.
    /// Null role disables it.
    /// </summary>
    Task<string> SetCurrentWeekRoleAsync(ulong guildId, ulong? roleId, int topAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the channel weekly winner announcements are sent to. Null disables announcements.
    /// </summary>
    Task<string> SetAnnouncementChannelAsync(ulong guildId, ulong? channelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the weekly winner announcement message. Null resets it to the default.
    /// Supports {user}, {count} and {week} placeholders.
    /// </summary>
    Task<string> SetAnnouncementMessageAsync(ulong guildId, string? message,
        CancellationToken cancellationToken = default);
}
