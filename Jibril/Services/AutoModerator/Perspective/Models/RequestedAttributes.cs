using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class RequestedAttributes
    {
        public TOXICITY TOXICITY;
    }

    public class TOXICITY
    {
        public float value;
        public string type;
        public IList<SpanScore> SpanScores { get; set; }
        public SummaryScore SummaryScore { get; set; }
    }
}
