using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Hungergameconfig
    {
        public ulong Guild { get; set; }
        public ulong MsgId { get; set; }
        public bool Live { get; set; }
        public int Round { get; set; }
        public DateTime SignupDuration { get; set; }
        public bool Signupstage { get; set; }
    }
}
