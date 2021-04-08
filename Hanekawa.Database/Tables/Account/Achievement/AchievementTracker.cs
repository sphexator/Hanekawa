namespace Hanekawa.Database.Tables.Account.Achievement
{
    public class AchievementTracker
    {
        public int Type { get; set; }
        public ulong UserId { get; set; }
        public int Count { get; set; } = 1;
    }
}