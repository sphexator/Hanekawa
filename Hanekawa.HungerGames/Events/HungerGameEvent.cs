using System;

namespace Hanekawa.HungerGames.Events
{
    internal partial class HungerGameEvent
    {
        private readonly Random _random;

        public HungerGameEvent(Random random) => _random = random;
    }
}