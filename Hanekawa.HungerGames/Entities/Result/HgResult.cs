using HungerGame.Entities.User;
using System.IO;

namespace HungerGame.Entities
{
    public class HgResult
    {
        public UserAction Action { get; internal set; }
        public Stream Image { get; internal set; }
    }
}