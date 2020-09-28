using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
    {
        private readonly string[] _dieStrings =
        {
            "Fell down a tree and died",
            "Fell down a mountain and died",
            "Ate tons of berries and died of poisoning",
            "Got bit by a snake and decided to chop his leg off, bled to death",
            "I used to be interested in this game, but then I took an arrow to the knee"
        };

        public string Die(HungerGameProfile participant)
        {
            participant.Health = 0;
            participant.Alive = false;
            return _dieStrings[_random.Next(_dieStrings.Length)];
        }
    }
}