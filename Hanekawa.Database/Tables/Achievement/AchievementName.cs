namespace Hanekawa.Database.Tables.Achievement
{
    public class AchievementName
    {
        public int AchievementNameId { get; set; }
        public string Name { get; set; } = "Test Name";
        public string Description { get; set; } = "Test Desc";
        public bool Stackable { get; set; } = false;
    }
}