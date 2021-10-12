using System;
using System.Collections.Generic;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Account.Achievement
{
    public class Achievement
    {
        public Guid AchievementId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public int Points { get; set; } = 10;
        public int? Reward { get; set; } = null;
        public int Requirement { get; set; } = 1;
        public bool Hidden { get; set; } = false;
        public AchievementCategory Category { get; set; }
        public AchievementDifficulty Difficulty { get; set; }
        
        public List<AccountAchievement> Unlocked { get; set; }
    }
}