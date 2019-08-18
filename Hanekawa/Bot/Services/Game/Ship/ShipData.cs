using System.Collections.Concurrent;
using Hanekawa.Database.Tables.BotGame;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> _existingBattles
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _activeBattles
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _activeDuels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();
    }
}