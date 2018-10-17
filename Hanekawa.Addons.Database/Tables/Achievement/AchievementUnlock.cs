using System.ComponentModel.DataAnnotations.Schema;

namespace Hanekawa.Addons.Database.Tables.Achievement
{
    public class AchievementUnlock
    {
        public int Id { get; set; }

        [ForeignKey("Achievement")]
        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; }

        public int TypeId { get; set; }
        public ulong UserId { get; set; }
    }
}