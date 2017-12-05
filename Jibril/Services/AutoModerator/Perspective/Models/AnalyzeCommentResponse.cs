using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class AnalyzeCommentResponse
    {
        public AttributeScores AttributeScores { get; set; }
        public IList<string> Languages { get; set; }
    }
}
