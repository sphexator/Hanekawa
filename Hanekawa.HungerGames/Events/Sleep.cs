﻿using Hanekawa.Entities.Game.HungerGame;

namespace Hanekawa.HungerGames.Events
{
    internal partial class HungerGameEvent
    {
        public string Sleep(HungerGameProfile participant)
        {
            participant.Tiredness += 80;
            participant.Stamina = 100;
            if (participant.Tiredness > 100) participant.Tiredness = 100;
            return "Fell asleep";
        }
    }
}