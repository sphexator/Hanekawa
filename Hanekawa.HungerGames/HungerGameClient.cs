using System.Threading.Tasks;

namespace Hanekawa.HungerGames
{
    public class HungerGameClient
    {
        private HungerGameConfig _config;
        public HungerGameClient(HungerGameConfig config) => _config = config;

        public void StartGame(){}

        public void NextRound(){}
    }
}