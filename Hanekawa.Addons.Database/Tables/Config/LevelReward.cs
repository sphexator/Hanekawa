namespace Hanekawa.Addons.Database.Tables.Config
{
    public class LevelReward
    {
        public ulong GuildId { get; set; }
        public int Level { get; set; }
        public ulong Role { get; set; }
        public bool Stackable { get; set; } = false;
    }
}