using Hanekawa.Entities;
using Hanekawa.Entities.Game.HungerGame;

namespace Hanekawa.HungerGames.Entities
{
    public class UserAction
    {
        public HungerGameProfile After { get; set; }
        public HungerGameProfile Before { get; set; }
        public ActionType Action { get; set; }
        public string Message { get; set; }
    }
}