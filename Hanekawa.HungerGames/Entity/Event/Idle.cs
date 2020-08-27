﻿using System;
using System.Collections.Generic;
using System.Text;
using Hanekawa.HungerGames.Entity.User;

namespace Hanekawa.HungerGames.Entity.Event
{
    internal class Idle
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
