using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Guildinfo
    {
        public string Guild { get; set; }
        public string Rules { get; set; }
        public string Faq { get; set; }
        public string Faq2 { get; set; }
        public ulong? Rulesmsgid { get; set; }
        public ulong? Faqmsgid { get; set; }
        public ulong? Faq2msgid { get; set; }
        public ulong? LevelInviteMsgId { get; set; }
        public ulong? Staffmsgid { get; set; }
    }
}
