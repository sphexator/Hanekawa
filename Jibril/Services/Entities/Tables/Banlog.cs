using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Banlog
    {
        public uint UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime UnbanDate { get; set; }
        public uint Counter { get; set; }
    }
}
