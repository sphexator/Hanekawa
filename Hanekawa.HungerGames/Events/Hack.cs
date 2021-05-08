using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.HungerGames.Events
{
    internal partial class HungerGameEvent
    {
        public string Hack(HungerGameProfile participant)
        {
            participant.Water += 10;
            participant.Food += 10;
            participant.FirstAid++;
            participant.Weapons++;
            participant.MeleeWeapon++;
            participant.RangeWeapon++;
            participant.Bullets += 10;
            return "Breached the border and smuggled tons of supplies";
        }
    }
}