using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class SpanScore
    {
        public int Begin { get; set; }
        public int End { get; set; }
        public Score Score { get; set; }
    }
}
