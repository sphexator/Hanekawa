using System.Collections.Generic;

namespace Hanekawa.Addons.Perspective.Models
{
    public class AnalyzeCommentResponse
    {
        public AttributeScores AttributeScores { get; set; }
        public IList<string> Languages { get; set; }
    }
}
