using Hanekawa.Addons.Database;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Entities.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Tables.BotGame;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService : INService
    {
        private readonly DbService _db;
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly ImageGenerator _img;

        public ShipGameService(DbService db, HttpClient client, Random random, ImageGenerator img)
        {
            _db = db;
            _client = client;
            _random = random;
            _img = img;

            foreach (var x in _db.GameEnemies)
            {
                if (x.Elite) _eliteEnemies.TryAdd(x.Id, x);
                else if (x.Rare) _rareEnemies.TryAdd(x.Id, x);
                else _regularEnemies.TryAdd(x.Id, x);
            }
        }

        public async Task SearchAsync(SocketGuildUser user)
        {
            if(!_activeBattles.TryGetValue(user.Guild.Id, out var battles)) return;
            if (battles.TryGetValue(user.Id, out _)) return;
            var enemy = GetEnemy();
            if (enemy == null)
            {

            }
            else
            {
                _activeBattles.TryAdd(x)
            }
        }

        public async Task PvPBattle()
        {

        }

        public async Task PvEBattle()
        {

        }

        private GameEnemy GetEnemy()
        {
            var chance = _random.Next(1000);
            if (chance <= 50) return _eliteEnemies[_random.Next(_eliteEnemies.Count)];
            if (chance <= 150) return _rareEnemies[_random.Next(_rareEnemies.Count)];
            if (chance > 150 && chance < 600) return _regularEnemies[_random.Next(_regularEnemies.Count)];
            return null;
        }
    }
}