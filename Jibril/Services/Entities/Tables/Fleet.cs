using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Fleet
    {
        public int Id { get; set; }
        public ulong? Userid { get; set; }
        public string Name { get; set; }
        public string Clubname { get; set; }
        public int? Rank { get; set; }
        public DateTime? Joindate { get; set; }
        public int? Clubid { get; set; }
    }
}
