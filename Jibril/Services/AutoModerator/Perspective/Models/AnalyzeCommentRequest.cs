using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class AnalyzeCommentRequest
    {
        public Comment comment { get; }
        public RequestedAttributes requestedAttributes { get; }
        public bool doNotStore;

        public AnalyzeCommentRequest(string msg)
        {
            comment = new Comment(msg);
            requestedAttributes = new RequestedAttributes();
            doNotStore = true;
        }
    }
}
