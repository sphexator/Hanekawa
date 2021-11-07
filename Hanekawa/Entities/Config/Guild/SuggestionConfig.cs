namespace Hanekawa.Entities.Config.Guild
{
    public class SuggestionConfig
    {
        public ulong GuildId { get; set; }
        public ulong? Channel { get; set; } = null;
        public string EmoteYes { get; set; }
        public string EmoteNo { get; set; }
    }
}