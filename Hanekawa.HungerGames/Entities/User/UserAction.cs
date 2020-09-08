namespace HungerGame.Entities.User
{
    public class UserAction
    {
        public HungerGameProfile AfterProfile { get; internal set; } = new HungerGameProfile();
        public HungerGameProfile BeforeProfile { get; internal set; } = new HungerGameProfile();
        public ActionType Action { get; internal set; } = ActionType.Idle;
        public object Reward { get; internal set;} = null;
    }
}