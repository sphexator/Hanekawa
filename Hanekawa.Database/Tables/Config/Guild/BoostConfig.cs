namespace Hanekawa.Database.Tables.Config.Guild
{
    public class BoostConfig
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; } = null;
        public string Message { get; set; } = null;
        public int CreditGain { get; set; } = 0;
        public int SpecialCreditGain { get; set; } = 0;
        public int ExpGain { get; set; } = 0;
    }
}