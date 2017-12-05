using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class AnalyzeCommentRequest
    {
        public Comment Comment { get; }
        public RequestedAttributes RequestedAttributes { get; }
        public Boolean DoNotStore = true;

        public AnalyzeCommentRequest(string msg)
        {
            Comment = new Comment(msg);
            RequestedAttributes = new RequestedAttributes();
        }
    }
}
