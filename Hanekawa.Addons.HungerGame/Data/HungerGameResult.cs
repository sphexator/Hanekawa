using Hanekawa.Addons.Database.Tables;
using System.Collections.Generic;
using System.IO;

namespace Hanekawa.Addons.HungerGame.Data
{
    public class HungerGameResult
    {
        public string Content { get; set; }
        public Stream Image { get; set; }
        public IEnumerable<HungerGameLive> Participants { get; set; }
    }
}
