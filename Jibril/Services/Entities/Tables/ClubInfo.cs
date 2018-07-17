using System;

namespace Jibril.Services.Entities.Tables
{
    public class ClubInfo
    {
        public uint Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Leader { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public ulong? Channel { get; set; }
        public ulong RoleId { get; set; }
    }
}
