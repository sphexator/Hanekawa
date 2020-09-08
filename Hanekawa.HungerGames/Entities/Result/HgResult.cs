using System.IO;
using Hanekawa.HungerGames.Entities.User;

namespace Hanekawa.HungerGames.Entities.Result
{
    public class HgResult
    {
        public UserAction Action { get; internal set; }
        public Stream Image { get; internal set; }
    }
}