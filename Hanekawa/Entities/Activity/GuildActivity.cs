namespace Hanekawa.Entities.Activity;

/// <summary>
/// Tracks how many rate-limited messages a user has sent in a given week (Monday start).
/// </summary>
public class GuildActivity
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public DateOnly WeekStart { get; set; }
    public long MessageCount { get; set; }
}
