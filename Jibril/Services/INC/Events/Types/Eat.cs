using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Eat
    {
        public static string EatEvent(Player player, Consumables consumables)
        {
            if (consumables.Fish > 0)
            {
                DatabaseHungerGame.EatFood(player.UserId);
                DatabaseHungerGame.ConsumeFood(player.UserId, ConsumableNames.Food[2]);
                return "Ate fish";
            }
            if (consumables.Beans > 0)
            {
                DatabaseHungerGame.EatFood(player.UserId);
                DatabaseHungerGame.ConsumeFood(player.UserId, ConsumableNames.Food[0]);
                return "Ate Beans";
            }
            if (consumables.Pasta > 0)
            {
                DatabaseHungerGame.EatFood(player.UserId);
                DatabaseHungerGame.ConsumeFood(player.UserId, ConsumableNames.Food[1]);
                return "Ate Pasta";
            }
            if (consumables.Ramen > 0)
            {
                DatabaseHungerGame.EatSpecialFood(player.UserId, Ramen.StaminaGain);
                DatabaseHungerGame.ConsumeFood(player.UserId, ConsumableNames.Food[3]);
                return "Ate Ramen";
            }

            return null;
        }
    }
}
