namespace Hanekawa.Perspective.Models
{
    internal class AnalyzeCommentRequest
    {
        private bool _doNotStore;

        internal AnalyzeCommentRequest(string msg)
        {
            comment = new Comment(msg);
            requestedAttributes = new RequestedAttributes();
            _doNotStore = true;
        }
#pragma warning disable IDE1006 // Naming Styles
        private Comment comment { get; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable IDE1006 // Naming Styles
        private RequestedAttributes requestedAttributes { get; }
#pragma warning restore IDE1006 // Naming Styles
    }
}