using System;

namespace Hanekawa.Addons.Database.Tables.Moderation
{
    public class ModLog
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string Action { get; set; }
        public ulong MessageId { get; set; }
        public ulong? ModId { get; set; }
        public string Response { get; set; }
        public DateTime Date { get; set; }
    }
}