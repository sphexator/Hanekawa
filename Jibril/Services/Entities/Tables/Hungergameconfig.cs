using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Hungergameconfig
    {
        public ulong? Guild { get; set; }
        public ulong? MsgId { get; set; }
        public sbyte Live { get; set; }
        public int? Round { get; set; }
        public DateTime? SignupDuration { get; set; }
        public sbyte? Signupstage { get; set; }
    }
}
