using System;
using System.Collections.Generic;

namespace Hanekawa.Entities.Users;

public class User
{
    public ulong Id { get; set; }

    public DateTimeOffset? PremiumExpiration { get; set; } = null;
    
    public List<GuildUser> GuildUsers { get; set; }
}