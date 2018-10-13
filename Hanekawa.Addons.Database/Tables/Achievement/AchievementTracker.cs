namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementTracker
    {
        public int AchievementTrackId { get; set; }
        public int AchievementId { get; set; }
        public ulong UserId { get; set; }
        public int CountToUnlock { get; set; }
        public bool Achieved { get; set; }
    }
}