using HungerGame.Entities.User;

namespace HungerGame.Entities.Internal.Events
{
    internal class Sleep : IRequired
    {
        internal void SleepEvent(HungerGameProfile profile)
        {
            profile.Tiredness = 0;
            profile.Stamina = 100;
        }
    }
}