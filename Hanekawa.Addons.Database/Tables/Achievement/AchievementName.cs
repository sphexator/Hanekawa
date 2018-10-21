namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementName
    {
        public int AchievementNameId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Stackable { get; set; }
    }
}