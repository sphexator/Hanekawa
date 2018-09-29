using System;

namespace Hanekawa.Addons.Database.Tables
{
    public class Suggestion
    {
        public uint Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool Status { get; set; }
        public ulong? MessageId { get; set; }
        public ulong? ResponseUser { get; set; }
        public string Response { get; set; }
        public DateTime Date { get; set; }
    }
}