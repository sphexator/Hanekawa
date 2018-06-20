using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Entities.Tables
{
    public class ClubPlayer
    {
        public uint ClubId { get; set; }
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public uint Rank { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
