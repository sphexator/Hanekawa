using System.Collections.Generic;

namespace Hanekawa.Services.AutoModerator.Perspective.Models
{
    public class AnalyzeCommentResponse
    {
        public AttributeScores AttributeScores { get; set; }
        public IList<string> Languages { get; set; }
    }
}