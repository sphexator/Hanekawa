using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Fleetinfo
    {
        public int Id { get; set; }
        public string Clubname { get; set; }
        public int? Members { get; set; }
        public DateTime? Creationdate { get; set; }
        public ulong? Leader { get; set; }
        public sbyte? Channel { get; set; }
        public ulong? Channelid { get; set; }
        public ulong? Roleid { get; set; }
    }
}
