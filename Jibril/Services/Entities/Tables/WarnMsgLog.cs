using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Entities.Tables
{
    public class WarnMsgLog
    {
        public int Id { get; set; }
        public int WarnId { get; set; }
        public ulong UserId { get; set; }
        public ulong MsgId { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
}
