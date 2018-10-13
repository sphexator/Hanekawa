﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Requirement { get; set; }
        public int? Reward { get; set; }
        public string ImageUrl { get; set; }

        [ForeignKey("AchievementType")]
        public int TypeId { get; set; }
        public AchievementType AchievementType { get; set; }

        [ForeignKey("AchievementDifficulty")]
        public int DifficultyId { get; set; }
        public AchievementDifficulty AchievementDifficulty { get; set; }
    }
}