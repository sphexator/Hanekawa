using Hanekawa.Addons.Database;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Entities.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task SearchAsync()
        {

        }

        public async Task PvPBattle()
        {

        }

        public async Task PvEBattle()
        {

        }
    }
}
