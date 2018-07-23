﻿namespace Jibril.Services.Entities.Tables
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        public ulong? WelcomeChannel { get; set; }
        public ulong? SuggestionChannel { get; set; }
        public ulong? BoardChannel { get; set; }
        public ulong? ReportChannel { get; set; }
        public ulong? EventChannel { get; set; }
        public ulong? EventSchedulerChannel { get; set; }
        public ulong? ModChannel { get; set; }
        public ulong? MusicChannel { get; set; }
        public ulong? MusicVcChannel { get; set; }
        public uint WelcomeLimit { get; set; }
        public bool WelcomeBanner { get; set; }
        public string WelcomeMessage { get; set; }
        public ulong? LogJoin { get; set; }
        public ulong? LogMsg { get; set; }
        public ulong? LogBan { get; set; }
        public ulong? LogAvi { get; set; }
        public uint ExpMultiplier { get; set; }
        public bool StackLvlRoles { get; set; }
        public ulong? MuteRole { get; set; }
        public bool FilterInvites { get; set; }
        public bool IgnoreAllChannels { get; set; }
    }
}
