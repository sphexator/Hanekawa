using Hanekawa.Entities.Game.HungerGame;

namespace Hanekawa.HungerGames.Events
{
    internal partial class HungerGameEvent
    {
        public string EatAndDrink(HungerGameProfile participant)
        {
            if (participant.Water > 0 && participant.Food > 0)
            {
                participant.Water--;
                participant.Food--;
                return "Ate some beans and drank water";
            }

            if (participant.Water > 0)
            {
                participant.Water--;
                return "Drank water";
            }

            participant.Food--;
            return "Ate beans";
        }
    }
}
