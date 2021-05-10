using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class BoostConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelId { get; set; } = null;
        public string Webhook { get; set; }
        public string Message { get; set; } = null;
        public int CreditGain { get; set; } = 0;
        public int SpecialCreditGain { get; set; } = 0;
        public int ExpGain { get; set; } = 0;
    }
}