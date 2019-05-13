namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class LoggingConfig
    {
        public ulong GuildId { get; set; }
        public ulong? LogJoin { get; set; } = null;
        public ulong? LogMsg { get; set; } = null;
        public ulong? LogBan { get; set; } = null;
        public ulong? LogAvi { get; set; } = null;
        public ulong? LogWarn { get; set; } = null;
        public ulong? LogAutoMod { get; set; } = null;
    }
}