using System.Collections.Generic;

namespace Hanekawa.WebUI.Models
{
    public class Command
    {
        public string Name { get; set; }
        public List<string> Commands { get; set; }
        public string Description { get; set; }
        public List<string> Example { get; set; }
        public bool Premium { get; set; }
        public List<string> Permissions { get; set; }
    }
}