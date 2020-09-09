using Hanekawa.HungerGames.Entities.User;

namespace Hanekawa.HungerGames.Entities.Internal.Events
{
    internal class Sleep
    {
        internal void SleepEvent(HungerGameProfile profile)
        {
            profile.Tiredness = 0;
            profile.Stamina = 100;
        }
    }
}