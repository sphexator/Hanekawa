using HungerGame.Entities.User;
using System.Collections.Generic;
using System.IO;

namespace HungerGame.Entities
{
    public class HgOverallResult
    {
        public List<UserAction> UserAction { get; internal set; }
        public Stream Image { get; internal set; }
        public List<HungerGameProfile> Participants { get; internal set; }
    }
}
