using Hanekawa.Addons.Database.Tables.BotGame;
using System.Collections.Concurrent;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private readonly ConcurrentDictionary<int, GameEnemy> _regularEnemies
            = new ConcurrentDictionary<int, GameEnemy>();

        private readonly ConcurrentDictionary<int, GameEnemy> _rareEnemies
            = new ConcurrentDictionary<int, GameEnemy>();

        private readonly ConcurrentDictionary<int, GameEnemy> _eliteEnemies
            = new ConcurrentDictionary<int, GameEnemy>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> _existingBattles
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _activeBattles
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _activeDuels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();
    }
}
