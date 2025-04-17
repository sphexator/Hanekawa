using System;
using System.Collections.Generic;
using Hanekawa.Entities.Users;

namespace Hanekawa.Entities.Club;

public class Club
{
    public ulong GuildId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ulong OwnerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ClubMember> Members { get; set; } = new List<ClubMember>();
}