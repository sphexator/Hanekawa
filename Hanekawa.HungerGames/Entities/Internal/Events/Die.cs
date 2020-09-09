using System;
using Hanekawa.HungerGames.Entities.User;

namespace Hanekawa.HungerGames.Entities.Internal.Events
{
    internal class Die
    {
        private readonly string[] _dieResponseStrings =
        {
            "Climbed up a tree and fell to his death",
            "Got bit by a snake and decided to chop his leg off, bleed to death",
            "I used to be interested in this game, but then I took an arrow to the knee"
        };

        private readonly Random _random;

        internal Die(Random random) => _random = random;

        internal void DieEvent(HungerGameProfile profile)
        {
            profile.Alive = false;
            profile.Health = 0;
        }
    }
}