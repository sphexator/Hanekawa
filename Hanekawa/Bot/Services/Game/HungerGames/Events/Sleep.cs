using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
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