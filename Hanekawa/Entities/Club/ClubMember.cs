using System;
using Hanekawa.Entities.Users;

namespace Hanekawa.Entities.Club;

public class ClubMember
{
    public ulong GuildId { get; set; }
    public string ClubName { get; set; } = null!;
    public ulong UserId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    
    // Navigation properties
    public virtual Club Club { get; set; } = null!;
    public virtual GuildUser User { get; set; } = null!;
}