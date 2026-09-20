using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class ActivityConfig : IConfig
{
    public ActivityConfig() { }

    public ActivityConfig(ulong guildId) => GuildId = guildId;

    [Key]
    public ulong GuildId { get; init; }

    /// <summary>
    /// Role handed to the most active user of the previous week when a week rolls over.
    /// </summary>
    public ulong? PreviousWeekRoleId { get; set; }

    /// <summary>
    /// Role held by the current week's most active users. Moves as the leaderboard changes.
    /// </summary>
    public ulong? CurrentWeekRoleId { get; set; }

    /// <summary>
    /// How many of the current week's most active users hold <see cref="CurrentWeekRoleId"/>.
    /// </summary>
    public int CurrentWeekTopAmount { get; set; } = 1;

    /// <summary>
    /// Channel the weekly winner announcement is sent to when a week rolls over.
    /// </summary>
    public ulong? AnnouncementChannelId { get; set; }

    /// <summary>
    /// Announcement message. Supports {user} (mention), {count} (messages) and {week} (week start date).
    /// </summary>
    public string? AnnouncementMessage { get; set; }

    /// <summary>
    /// Users currently holding <see cref="CurrentWeekRoleId"/>.
    /// </summary>
    public List<ulong> CurrentHolders { get; set; } = [];

    /// <summary>
    /// User currently holding <see cref="PreviousWeekRoleId"/>.
    /// </summary>
    public ulong? PreviousWeekHolderId { get; set; }

    /// <summary>
    /// Last week (Monday) the rollover was processed for.
    /// </summary>
    public DateOnly? LastProcessedWeekStart { get; set; }

    [JsonIgnore]
    public GuildConfig? GuildConfig { get; set; }
}
