using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services.Games.ShipGame;
using Jibril.Services.Games.ShipGame.Data;

namespace Jibril.Modules.Game
{
    public class ShipGame : InteractiveBase
    {
        private readonly BaseStats _baseStats;
        private readonly ClassStats _classStats;
        private readonly EnemyStat _enemyStat;
        
        public ShipGame(BaseStats baseStats, ClassStats classStats, EnemyStat enemyStat)
        {
            _baseStats = baseStats;
            _classStats = classStats;
            _enemyStat = enemyStat;
        }

        [Command("search", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task ShipGameSearchAsync()
        {

        }
    }
}
