using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class CurrencyConfig
    {
        public Snowflake GuildId { get; set; }
        public bool EmoteCurrency { get; set; } = false;
        public string CurrencyName { get; set; } = "Credit";
        public string CurrencySign { get; set; } = "$";
        public Snowflake? CurrencySignId { get; set; }
        public bool SpecialEmoteCurrency { get; set; } = false;
        public string SpecialCurrencyName { get; set; } = "Special credit";
        public string SpecialCurrencySign { get; set; } = "$";
        public Snowflake? SpecialCurrencySignId { get; set; }
    }
}
