namespace Hanekawa.Database.Tables.Achievement
{
    public class AchievementMeta
    {
        public int AchievementId { get; set; }
        public string Name { get; set; } = "Test";
        public string Description { get; set; } = "Test";
        public int Requirement { get; set; } = 1;
        public bool Once { get; set; } = true;
        public int? Reward { get; set; } = null;
        public int Points { get; set; } = 10;
        public string ImageUrl { get; set; } = "test";
        public bool Hidden { get; set; } = false;
        public bool Global { get; set; } = true;

        public int AchievementNameId { get; set; }
        public AchievementName AchievementName { get; set; }

        public int TypeId { get; set; }
        public AchievementType AchievementType { get; set; }

        public AchievementDifficulty AchievementDifficulty { get; set; }
    }
}