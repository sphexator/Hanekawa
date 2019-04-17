namespace Hanekawa.Database.Tables.Stats
{
    public class GuildStats
    {
        public ulong GuildId { get; set; }
        public int StarAmount { get; set; } = 0;
        public int StarMessages { get; set; } = 0;
    }
}