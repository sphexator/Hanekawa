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
        public static string EatEvent(Profile profile)
        {
            if (profile.Consumables.Fish > 0)
            {
                DatabaseHungerGame.EatFood(profile.Player.UserId);
                DatabaseHungerGame.ConsumeFood(profile.Player.UserId, ConsumableNames.Food[2]);
                return "Ate fish";
            }
            if (profile.Consumables.Beans > 0)
            {
                DatabaseHungerGame.EatFood(profile.Player.UserId);
                DatabaseHungerGame.ConsumeFood(profile.Player.UserId, ConsumableNames.Food[0]);
                return "Ate Beans";
            }
            if (profile.Consumables.Pasta > 0)
            {
                DatabaseHungerGame.EatFood(profile.Player.UserId);
                DatabaseHungerGame.ConsumeFood(profile.Player.UserId, ConsumableNames.Food[1]);
                return "Ate Pasta";
            }

            if (profile.Consumables.Ramen <= 0) return null;
            DatabaseHungerGame.EatSpecialFood(profile.Player.UserId, Ramen.StaminaGain);
            DatabaseHungerGame.ConsumeFood(profile.Player.UserId, ConsumableNames.Food[3]);
            return "Ate Ramen";

        }
    }
}
