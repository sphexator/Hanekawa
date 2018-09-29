namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class LevelReward
    {
        public ulong GuildId { get; set; }
        public uint Level { get; set; }
        public ulong Role { get; set; }
        public bool Stackable { get; set; }
    }
}