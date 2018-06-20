using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Entities.Tables
{
    public class GuildInfo
    {
        public ulong GuildId { get; set; }
        public ulong RuleMessageId { get; set; }
        public string Rules { get; set; }

        public ulong FaqOneMessageId { get; set; }
        public string FaqOne { get; set; }
        public ulong FaqTwoMessageId { get; set; }
        public string FaqTwo { get; set; }

        public ulong StaffMessageId { get; set; }
        public ulong LevelMessageId { get; set; }
        public ulong InviteMessageId { get; set; }
    }
}