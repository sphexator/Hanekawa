namespace Jibril.Services.Entities.Tables
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public bool Welcome { get; set; }
        public ulong WelcomeChannel { get; set; }
        public uint WelcomeLimit { get; set; }
        public ulong? LogJoin { get; set; }
        public ulong? LogMsg { get; set; }
        public ulong? LogBan { get; set; }
        public ulong? LogMute { get; set; }
        public uint? AntiSpam { get; set; }
    }
}
