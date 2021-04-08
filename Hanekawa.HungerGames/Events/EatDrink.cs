using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.HungerGames.Events
{
    public partial class HungerGameEvent
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
