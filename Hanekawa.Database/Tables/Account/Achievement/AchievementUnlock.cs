using System;
using Disqord;

namespace Hanekawa.Database.Tables.Account.Achievement
{
    public class AchievementUnlocked
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        
        public Snowflake UserId { get; set; }
        public AccountGlobal Account { get; set; }
        
        public Guid AchieveId { get; set; }
        public Achievement Achievement { get; set; }
    }
}