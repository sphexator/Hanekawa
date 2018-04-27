using System;

namespace Jibril.Modules.Administration.List
{
    public class WarnList
    {
        public int Id { get; set; }
        public string StaffId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }

    public class WarnAmount
    {
        public uint Warnings { get; set; }
        public uint TotalWarnings { get; set; }
    }

    public class MsgLog
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong MsgId { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}