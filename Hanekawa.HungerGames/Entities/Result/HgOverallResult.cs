using System.Collections.Generic;
using System.IO;
using Hanekawa.HungerGames.Entities.User;

namespace Hanekawa.HungerGames.Entities.Result
{
    public class HgOverallResult
    {
        public List<UserAction> UserAction { get; internal set; }
        public Stream Image { get; internal set; }
        public List<HungerGameProfile> Participants { get; internal set; }
    }
}
