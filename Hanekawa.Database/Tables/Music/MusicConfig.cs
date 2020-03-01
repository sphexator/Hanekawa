namespace Hanekawa.Database.Tables.Music
{
    public class MusicConfig
    {
        public ulong GuildId { get; set; }
        public ulong? TextChId { get; set; } = null;
        public ulong? VoiceChId { get; set; } = null;
    }
}