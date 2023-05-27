using System;

namespace Hanekawa.Entities.Users;

public class Warning
{
    public Guid Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong ModeratorId { get; set; }
    public string Reason { get; set; } = "No reason provided.";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Valid { get; set; } = true;
}