using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Users;

public class GuildUser : IMemberEntity
{
    public GuildUser()
    {
        User = new();
    }
    
    public GuildUser(ulong guildId, ulong userId)
    {
        GuildId = guildId;
        UserId = userId;
        User = new() { Id = userId };
    }
    
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public long NextLevelExperience { get; set; } = 100;
    public long CurrentLevelExperience { get; set; } = 0;

    public DateTimeOffset DailyClaimed { get; set; } = DateTimeOffset.MinValue;
    public int DailyStreak { get; set; } = 0;
    
    public TimeSpan TotalVoiceTime { get; set; } = TimeSpan.Zero;
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;
    
    public User User { get; set; }
}