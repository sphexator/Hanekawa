using System;

namespace Hanekawa.Services.Entities.Tables.Stats
{
    public class JoinStat
    {
        public ulong GuildId { get; set; }
        public DateTime LatestEntry { get; set; }

        public int Join { get; set; }
        public int Leave { get; set; }

        public int DayOne { get; set; }
        public DateTime DayOneDate { get; set; }

        public int DayTwo { get; set; }
        public DateTime DayTwoDate { get; set; }

        public int DayThree { get; set; }
        public DateTime DayThreeDate { get; set; }

        public int DayFour { get; set; }
        public DateTime DayFourDate { get; set; }

        public int DayFive { get; set; }
        public DateTime DayFiveDate { get; set; }

        public int DaySix { get; set; }
        public DateTime DaySixDate { get; set; }

        public int DaySeven { get; set; }
        public DateTime DaySevenDate { get; set; }
    }
}
