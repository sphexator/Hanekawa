using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;

namespace Hanekawa.Tests.Common;

public static class TestUsers
{
    public static readonly DiscordMember TestMember = new()
    {
        Id = 1,
        Username = "Test-User",
        Nickname =  "Test-Nick",
        IsBot = false,
        RoleIds = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16],
        AvatarUrl = string.Empty,
        VoiceSessionId = null,
        GuildId = 1,
        Guild = new Guild
        {
            GuildId = 1,
            Description = string.Empty,
            Emotes = [
                new Emote {
                    Id = 1,
                    Name = "Test-Emote",
                    Format = "",
                    IsAnimated = false,
                    IsAvailable = false,
                    IsManaged = false
                },
                new Emote
                {
                    Id = 2,
                    Name = "Test-Emote-2",
                    Format = "",
                    IsAnimated = false,
                    IsAvailable = false,
                    IsManaged = false
                }
            ],
            Name = "Test-Guild",
            MemberCount = 1,
            EmoteCount = 1,
            IconUrl = string.Empty
        }
    };

     public static readonly GuildUser TestUser = new()
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
             GuildUsers = [TestUser]
         }
     };
}