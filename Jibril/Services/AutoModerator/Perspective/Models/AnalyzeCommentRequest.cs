using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class AnalyzeCommentRequest
    {
#pragma warning disable IDE1006 // Naming Styles
        public Comment comment { get; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable IDE1006 // Naming Styles
        public RequestedAttributes requestedAttributes { get; }
#pragma warning restore IDE1006 // Naming Styles
        public bool doNotStore;

        public AnalyzeCommentRequest(string msg)
        {
            comment = new Comment(msg);
            requestedAttributes = new RequestedAttributes();
            doNotStore = true;
        }
    }
}
