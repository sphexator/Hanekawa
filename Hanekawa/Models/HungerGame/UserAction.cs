using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Models.HungerGame
{
    public class UserAction
    {
        public UserAction(HungerGameProfile before, HungerGameProfile after)
        {
            AfterProfile = after;
            BeforeProfile = before;
        }

        public HungerGameProfile AfterProfile { get; set; }
        public HungerGameProfile BeforeProfile { get; set; }
        public ActionType Action { get; internal set; } = ActionType.Idle;
        public object Reward { get; internal set;} = null;
    }
}