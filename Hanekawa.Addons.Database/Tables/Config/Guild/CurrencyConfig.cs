namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class CurrencyConfig
    {
        public ulong GuildId { get; set; }
        public bool EmoteCurrency { get; set; } = false;
        public string CurrencyName { get; set; } = "Credit";
        public string CurrencySign { get; set; } = "$";
        public bool SpecialEmoteCurrency { get; set; } = false;
        public string SpecialCurrencyName { get; set; } = "Special credit";
        public string SpecialCurrencySign { get; set; } = "$";
    }
}
