namespace Hanekawa.HungerGames.Entity.User
{
    public class UserAction
    {
        public UserAction(Participant before, Participant after)
        {
            AfterProfile = after;
            BeforeProfile = before;
        }

        public Participant AfterProfile { get; internal set; }
        public Participant BeforeProfile { get; internal set; }
        public ActionType Action { get; internal set; } = ActionType.Idle;
        public object Reward { get; internal set; } = null;
    }
}
