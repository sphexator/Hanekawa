using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Users;

public class User : IEntity
{
    public ulong Id { get; set; }
    public DateTimeOffset? PremiumExpiration { get; set; }
    public List<GuildUser> GuildUsers { get; set; } = [];
}