namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class LevelConfig
    {
        public ulong GuildId { get; set; }
        public int ExpMultiplier { get; set; } = 1;
        public bool StackLvlRoles { get; set; } = true;
    }
}
