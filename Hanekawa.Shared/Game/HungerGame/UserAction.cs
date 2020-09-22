namespace Hanekawa.Shared.Game.HungerGame
{
    public class UserAction
    {
        public HungerGameProfile AfterProfile { get; set; } = new HungerGameProfile();
        public HungerGameProfile BeforeProfile { get; set; } = new HungerGameProfile();
        public ActionType Action { get; internal set; } = ActionType.Idle;
        public object Reward { get; internal set;} = null;
    }
}