namespace Hanekawa.HungerGames.Entity.Event
{
    internal class Sleep
    {
        internal void SleepEvent(Participant profile)
        {
            profile.Tiredness = 0;
            profile.Stamina = 100;
        }
    }
}