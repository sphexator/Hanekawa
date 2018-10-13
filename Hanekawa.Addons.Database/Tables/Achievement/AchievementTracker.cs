namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementTracker
    {
        public string Type { get; set; }
        public ulong UserId { get; set; }
        public int Count { get; set; }
    }
}