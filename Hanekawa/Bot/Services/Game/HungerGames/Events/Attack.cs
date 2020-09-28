using System.Collections.Generic;
using System.Linq;
using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
    {
        public string Attack(HungerGameProfile participant, List<HungerGameProfile> targets)
        {
            var alive = targets.Where(x => x.Alive && x.Health > 0);
            var target =  targets[_random.Next(alive.Count())];
            int dmg;
            var criticalChance = _random.Next(100);
            if (participant.RangeWeapon > 0 && participant.Bullets > 0)
            {
                participant.Bullets--;
                dmg = _random.Next(30, 61);
                if (criticalChance <= 20) dmg *= 2;
                if (target.Health - dmg <= 0)
                {
                    target.Health = 0;
                    target.Alive = false;
                    return $"Shot and killed {target.Name} with a bow";
                }
                target.Health -= dmg;
                return $"Shot {target.Name} with a bow";
            }

            if (participant.MeleeWeapon > 0)
            {
                dmg = _random.Next(10, 21);
                if (criticalChance <= 20) dmg *= 2;
                if (target.Health - dmg <= 0)
                {
                    target.Health = 0;
                    target.Alive = false;
                    return $"Slammed and killed {target.Name} with a hammer";
                }
                target.Health -= dmg;
                return $"Slammed {target.Name} with a hammer";
            }

            dmg = _random.Next(5, 11);
            if (target.Health - dmg <= 0)
            {
                target.Health = 0;
                target.Alive = false;
                return $"Bitch slapped and killed {target.Name}";
            }
            target.Health -= dmg;
            return $"Bitch slapped {target.Name}";
        }
    }
}