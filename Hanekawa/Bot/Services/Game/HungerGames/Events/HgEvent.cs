using System;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent : INService
    {
        private readonly Random _random;

        public HgEvent(Random random) => _random = random;
    }
}
