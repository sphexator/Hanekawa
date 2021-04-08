using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Account.HungerGame;

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