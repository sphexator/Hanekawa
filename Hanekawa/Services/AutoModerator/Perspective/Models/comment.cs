namespace Hanekawa.Services.AutoModerator.Perspective.Models
{
    public class Comment
    {
#pragma warning disable IDE1006 // Naming Styles
        public string text { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public Comment(string text, string type = "PLAIN_TEXT")
        {
            this.text = text;
            this.type = type;
        }
    }
}

