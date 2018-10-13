namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Difficulty { get; set; }
        public int Requirement { get; set; }
        public int? Reward { get; set; }
        public string ImageUrl { get; set; }
    }
}