using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Mute
    {
        public ulong Guildid { get; set; }
        public ulong UserId { get; set; }
        public DateTime? Time { get; set; }
    }
}
