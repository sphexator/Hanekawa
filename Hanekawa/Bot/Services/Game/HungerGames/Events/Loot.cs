using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
    {
        private const int FoodAndWater = 100;
        private const int Weapons = 15;

        public string Loot(HungerGameProfile participant)
        {
            const int pool = FoodAndWater + Weapons;
            var result = _random.Next(pool);
            if (result <= FoodAndWater)
            {
                participant.Food++;
                participant.Water++;
                return "Found some food and water in a crate";
            }

            const int range = 50;
            const int melee = 25;
            result = _random.Next(range + melee);
            if (result >= range)
            {
                participant.Weapons++;
                participant.MeleeWeapon++;
                return "Found a hammer in a crate";
            }

            if (participant.RangeWeapon > 0)
            {
                participant.Weapons++;
                participant.RangeWeapon++;
                participant.Bullets += 10;
                return "Found a bow and arrows in a crate";
            }

            participant.Bullets += 10;
            return "Found 10 arrows in a crate";
        }
    }
}
