using System.Collections.Concurrent;
using Hanekawa.Database.Tables.BotGame;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private readonly ConcurrentDictionary<ulong, MemoryCache> _activeBattles
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _activeDuels
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private readonly ConcurrentDictionary<int, GameEnemy> _eliteEnemies
            = new ConcurrentDictionary<int, GameEnemy>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _existingBattles
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private readonly ConcurrentDictionary<int, GameEnemy> _rareEnemies
            = new ConcurrentDictionary<int, GameEnemy>();

        private readonly ConcurrentDictionary<int, GameEnemy> _regularEnemies
            = new ConcurrentDictionary<int, GameEnemy>();
    }
}