namespace Hanekawa.Entities.Users;

public class User
{
    public User()
    {
        GuildUsers = [];
    }
    
    public ulong Id { get; set; }
    public DateTimeOffset? PremiumExpiration { get; set; } = null;
    public List<GuildUser> GuildUsers { get; set; }
}