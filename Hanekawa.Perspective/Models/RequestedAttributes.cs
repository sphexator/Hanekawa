using System.Collections.Generic;

namespace Hanekawa.Perspective.Models
{
    public class RequestedAttributes
    {
        public TOXICITY TOXICITY;
    }

    public class TOXICITY
    {
        public string type;
        public float value;
        public IList<SpanScore> SpanScores { get; set; }
        public SummaryScore SummaryScore { get; set; }
    }
}