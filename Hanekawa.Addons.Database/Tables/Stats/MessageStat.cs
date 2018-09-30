namespace Hanekawa.Addons.Database.Tables.Stats
{
    public class MessageStat
    {
        public ulong GuildId { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
        public string WeekOne { get; set; }
        public string WeekTwo { get; set; }
        public string WeekThree { get; set; }
        public string WeekFour { get; set; }
    }
}
