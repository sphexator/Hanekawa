using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementMeta
    {
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Requirement { get; set; }
        public bool Once { get; set; }
        public int? Reward { get; set; }
        public int Points { get; set; }
        public string ImageUrl { get; set; }
        public bool Hidden { get; set; }
        public bool Global { get; set; }

        public int AchievementNameId { get; set; }
        public AchievementName AchievementName { get; set; }

        public int TypeId { get; set; }
        public AchievementType AchievementType { get; set; }

        public int DifficultyId { get; set; }
        public AchievementDifficulty AchievementDifficulty { get; set; }
    }
}