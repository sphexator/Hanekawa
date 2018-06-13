using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Waifu
    {
        public ulong User { get; set; }
        public string Name { get; set; }
        public ulong? Claim { get; set; }
        public string Claimname { get; set; }
        public DateTime? Timer { get; set; }
        public int? Rank { get; set; }
    }
}
