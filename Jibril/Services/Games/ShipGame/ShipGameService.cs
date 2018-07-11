using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Jibril.Services.Entities.Tables;

namespace Jibril.Services.Games.ShipGame
{
    public class ShipGameService
    {
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> ActiveBattles { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        public ShipGameService() { }

        public bool isInBattle(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out var game);
            return check;
        }

        public GameEnemy GetEnemyData(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out var game);
            return game;
        }

        public void AddBattle(SocketCommandContext context, GameEnemy enemy)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryAdd(context.User.Id, enemy);
        }

        public void RemoveBattle(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);
        }
    }
}
