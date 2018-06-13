using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Warnmsglog
    {
        public int Warnmsglog1 { get; set; }
        public int? Id { get; set; }
        public ulong? Userid { get; set; }
        public ulong? Msgid { get; set; }
        public string Author { get; set; }
        public string Msg { get; set; }
        public DateTime? Datetime { get; set; }
    }
}
