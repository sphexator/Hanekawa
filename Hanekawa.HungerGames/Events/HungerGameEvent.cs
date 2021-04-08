using System;

namespace Hanekawa.HungerGames.Events
{
    public partial class HungerGameEvent
    {
        private readonly Random _random;

        public HungerGameEvent(Random random) => _random = random;
    }
}