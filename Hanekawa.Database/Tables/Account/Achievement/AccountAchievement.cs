using System;
using Disqord;

namespace Hanekawa.Database.Tables.Account.Achievement
{
    public class AccountAchievement
    {
        public DateTimeOffset DateAchieved { get; set; }
        
        public Snowflake UserId { get; set; }
        public AccountGlobal User { get; set; }
        
        public Guid AchievementId { get; set; }
        public Achievement Achievement { get; set; }
    }
}