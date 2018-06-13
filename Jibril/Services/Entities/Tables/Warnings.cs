using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Warnings
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public uint Warnings1 { get; set; }
        public uint TotalWarnings { get; set; }
    }
}
