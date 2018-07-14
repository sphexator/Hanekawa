namespace Jibril.Services.Entities.Tables
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        public ulong? WelcomeChannel { get; set; }
        public ulong? SuggestionChannel { get; set; }
        public ulong? BoardChannel { get; set; }
        public ulong? ReportChannel { get; set; }
        public uint WelcomeLimit { get; set; }
        public ulong? LogJoin { get; set; }
        public ulong? LogMsg { get; set; }
        public ulong? LogBan { get; set; }
        public ulong? LogAvi { get; set; }
        public uint? AntiSpam { get; set; }
        public uint ExpMultiplier { get; set; }
        public bool StackLvlRoles { get; set; }
        public ulong? MuteRole { get; set; }
    }
}
