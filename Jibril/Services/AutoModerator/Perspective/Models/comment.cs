using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.AutoModerator.Perspective.Models
{
    public class Comment
    {
        public string text { get; set; }
        public string type { get; set; }

        public Comment(string text, string type = "PLAIN_TEXT")
        {
            this.text = text;
            this.type = type;
        }
    }
}

