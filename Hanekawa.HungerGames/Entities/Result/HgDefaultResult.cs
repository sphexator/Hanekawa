using HungerGame.Entities.User;
using System.Collections.Generic;
using System.IO;

namespace HungerGame.Entities.Result
{
    public class HgDefaultResult
    {
        public UserAction Action { get; internal set; }
        public Stream Image { get; internal set; }
        public List<HungerGameProfile> Participants { get; internal set; }
    }
}
