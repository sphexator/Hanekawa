using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Models.HungerGame
{
    public class UserAction
    {
        public HungerGameProfile AfterProfile { get; set; }
        public HungerGameProfile BeforeProfile { get; set; }
        public ActionType Action { get; internal set; } = ActionType.Idle;
        public string Message { get; set; }
    }
}