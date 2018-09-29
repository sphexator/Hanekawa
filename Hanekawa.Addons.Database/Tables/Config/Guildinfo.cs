namespace Hanekawa.Addons.Database.Tables.Config
{
    public class GuildInfo
    {
        public ulong GuildId { get; set; }
        public ulong RuleChannelId { get; set; }
        public ulong OtherChannelId { get; set; }

        public ulong RuleMessageId { get; set; }
        public string Rules { get; set; }

        public ulong FaqOneMessageId { get; set; }
        public string FaqOne { get; set; }
        public ulong FaqTwoMessageId { get; set; }
        public string FaqTwo { get; set; }

        public ulong StaffMessageId { get; set; }
        public ulong LevelMessageId { get; set; }
        public ulong LinkMessageId { get; set; }
    }
}