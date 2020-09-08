using System;

namespace Hanekawa.HungerGames.Entities.Internal.Events
{
    internal class Meet : IRequired
    {
        private readonly string[] _meetEventStrings =
        {
            "Climbed up in a tree, seeing someone in the distance",
            "Lurks behind a tree, spying on someone"
        };

        private readonly Random _random;

        internal Meet(Random random) => _random = random;

        internal string MeetEvent() => _meetEventStrings[_random.Next(0, _meetEventStrings.Length)];
    }
}