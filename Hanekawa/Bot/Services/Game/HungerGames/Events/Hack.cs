﻿using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
    {
        public string Hack(HungerGameProfile participant)
        {
            participant.Water += 10;
            participant.Food += 10;
            participant.FirstAid++;
            participant.MeleeWeapon++;
            participant.RangeWeapon++;
            participant.Bullets += 10;
            return "Breached the border and smuggled tons of supplies";
        }
    }
}