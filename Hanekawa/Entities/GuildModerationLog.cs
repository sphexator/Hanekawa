using System;

namespace Hanekawa.Entities;

/// <summary>
/// Log for ban / mute / kick / etc
/// </summary>
public class GuildModerationLog
{
    /// <summary>
    /// Incremental ID tied to the guild
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Guild ID
    /// </summary>
    public ulong GuildId { get; set; }
    /// <summary>
    /// ID of user affected
    /// </summary>
    public ulong UserId { get; set; }
    /// <summary>
    /// Log message ID
    /// </summary>
    public ulong MessageId { get; set; }
    /// <summary>
    /// ID of moderator executing the action. Will not be a bot ID, 0 = Auto moderator
    /// </summary>
    public ulong? ModeratorId { get; set; }
    /// <summary>
    /// Reason for action
    /// </summary>
    public string Reason { get; set; } = "No reason provided";
    /// <summary>
    /// Date of action
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}