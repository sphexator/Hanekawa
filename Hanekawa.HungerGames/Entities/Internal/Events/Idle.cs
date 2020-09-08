using System;
using System.Reflection.Metadata.Ecma335;
using HungerGame.Entities.User;

namespace HungerGame.Entities.Internal.Events
{
    internal class Idle : IRequired
    {
        private readonly string[] _idleStrings =
        {
            "Looks at the sky, pondering about life",
            "frozen in time",
            "Standing still, looking at a tree",
            "Wonders if its possible to do ninjutsu"
        };

        private readonly Random _random;

        internal Idle(Random random) => _random = random;

        internal UserAction IdleEvent() => null;
    }
}