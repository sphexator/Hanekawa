using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Reaction
    {
        public int Id { get; set; }
        public string Msgid { get; set; }
        public string Chid { get; set; }
        public int? Counter { get; set; }
        public string Sent { get; set; }
    }
}
