namespace Hanekawa.Database.Tables.Config.Guild
{
    public class BoardConfig
    {
        public ulong GuildId { get; set; }
        public string Emote { get; set; }
        public ulong? Channel { get; set; } = null;
    }
}
