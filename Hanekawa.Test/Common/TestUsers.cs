using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;

namespace Hanekawa.Test.Common;

public static class TestUsers
{
    public static readonly DiscordMember TestMember = new DiscordMember
        {
            Id = 1,
            Username = "Test-User",
            Nickname =  "Test-Nick",
            IsBot = false,
            RoleIds = [],
            AvatarUrl = string.Empty,
            VoiceSessionId = null,
            GuildId = 1,
            Guild = new Guild
            {
                GuildId = 1,
                Description = string.Empty,
                Emotes = [],
                Name = "Test-Guild",
                MemberCount = 1,
                EmoteCount = 0,
                IconUrl = string.Empty
            }
        };

     public static readonly GuildUser TestUser = new GuildUser()
        {
            Id = 1,
            GuildId = 1,
            Currency = 0,
            Experience = 1,
            Level = 1,
            DailyClaimed = DateTimeOffset.UtcNow,
            DailyStreak = 0,
            LastSeen = DateTimeOffset.UtcNow,
            CurrentLevelExperience = 1,
            NextLevelExperience = 10,
            TotalVoiceTime = TimeSpan.Zero,
            User = new User
            {
                Id = 1,
                PremiumExpiration = DateTimeOffset.UtcNow.AddDays(-1),
                Inventory = [],
                GuildUsers = []
            }
        };   
}