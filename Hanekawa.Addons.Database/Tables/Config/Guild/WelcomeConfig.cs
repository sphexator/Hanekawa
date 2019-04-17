using System;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class WelcomeConfig
    {
        public ulong GuildId { get; set; }
        public ulong? Channel { get; set; }
        public int Limit { get; set; } = 4;
        public bool Banner { get; set; }
        public string Message { get; set; }
        public TimeSpan? TimeToDelete { get; set; }
        public bool AutoDelOnLeave { get; set; }
    }
}
