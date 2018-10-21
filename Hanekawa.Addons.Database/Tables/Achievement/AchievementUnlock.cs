using System.ComponentModel.DataAnnotations.Schema;

namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementUnlock
    {
        public int AchievementId { get; set; }
        public AchievementMeta Achievement { get; set; }

        public int TypeId { get; set; }
        public ulong UserId { get; set; }
    }
}