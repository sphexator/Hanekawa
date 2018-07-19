using System;

namespace Jibril.Services.Entities.Tables
{
    public class ModLog
    {
        public uint Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string Action { get; set; }
        public ulong MessageId { get; set; }
        public ulong? ModId { get; set; }
        public string Response { get; set; }
        public DateTime Date { get; set; }
    }
}