using System;

namespace Hanekawa.Entities
{
    public class ToxicityEntry
    {
        public double Value { get; set; }
        public ulong MessageId { get; set; }
        public DateTime Time { get; set; }
    }
}