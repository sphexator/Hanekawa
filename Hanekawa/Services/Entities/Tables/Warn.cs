using System;
using Hanekawa.Services.Administration;

namespace Hanekawa.Services.Entities.Tables
{
    public class Warn
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public WarnReason Type { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public ulong Moderator { get; set; }
        public bool Valid { get; set; }
        public TimeSpan? MuteTimer { get; set; }
    }
}
