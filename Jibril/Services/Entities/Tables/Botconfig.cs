using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Botconfig
    {
        public string Guild { get; set; }
        public string Welcome { get; set; }
        public string Botspam { get; set; }
        public string Botgame { get; set; }
        public string Suggestion { get; set; }
        public string Board { get; set; }
        public string Announcement { get; set; }
        public string Modlog { get; set; }
        public string Log { get; set; }
        public string Msglog { get; set; }
        public string Rules { get; set; }
        public sbyte? Raid { get; set; }
        public sbyte? RecruitmentStatus { get; set; }
    }
}
