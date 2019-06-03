using Hanekawa.Bot.Services.ImageGen;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService : INService
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly ImageGenerator _img;

        public ShipGameService(HttpClient client, Random random, ImageGenerator img)
        {
            _client = client;
            _random = random;
            _img = img;
            using (var db = new DbService())
            {
                foreach (var x in db.GameEnemies)
                {
                    if (x.Elite) _eliteEnemies.TryAdd(x.Id, x);
                    else if (x.Rare) _rareEnemies.TryAdd(x.Id, x);
                    else _regularEnemies.TryAdd(x.Id, x);
                }

                var cfg = db.GameConfigs.Find(1);
                if (cfg != null)
                {
                    DefaultDamage = cfg.DefaultDamage;
                    DefaultHealth = cfg.DefaultHealth;
                }
            }
        }

        public async Task<EmbedBuilder> SearchAsync(SocketGuildUser user, DbService db)
        {
            var battles = _existingBattles.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (battles.TryGetValue(user.Id, out _)) return new EmbedBuilder();
            var enemy = GetEnemy();
            if (enemy == null) return new EmbedBuilder().CreateDefault(
                    $"{user.GetName()} searched throughout the sea and didn't find anything", Color.Red.RawValue);
            
            battles.Set(user.Id, enemy, TimeSpan.FromHours(1));
            var embed = new EmbedBuilder().CreateDefault("You've encountered an enemy!\n" +
                                                    $"**{enemy.Name}**", Color.Green.RawValue);
            var userData = await db.GetOrCreateUserData(user);
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder { Name = "Type", Value = GetClassName(enemy.Id, db), IsInline = true },
                new EmbedFieldBuilder { Name = "Health", Value = GetHealth(userData.Level, enemy, await GetClassName(enemy.ClassId, db)), IsInline = true },
                new EmbedFieldBuilder { Name = "Level", Value = $"{userData.Level}", IsInline = true }
            };
            return embed;
        }

        public async Task PvPBattle(HanekawaContext context, SocketGuildUser user, int? bet = null)
        {
            using var db = new DbService();
            ShipGameUser playerOne;
            ShipGameUser playerTwo;
            var userDataOne = await db.GetOrCreateUserData(context.User);
            var userDataTwo = await db.GetOrCreateUserData(user);
            var coinFlip = _random.Next(2);
            if (coinFlip == 1)
            {
                playerOne = new ShipGameUser(context.User, await GetClassName(userDataOne.Class, db), bet);
                playerTwo = new ShipGameUser(user, await GetClassName(userDataTwo.Class, db), bet);
            }
            else
            {
                playerOne = new ShipGameUser(user, await GetClassName(userDataTwo.Class, db), bet);
                playerTwo = new ShipGameUser(context.User, await GetClassName(userDataOne.Class, db), bet);
            }
            var game = new ShipGame(playerOne, playerTwo);
            var msgLog = new LinkedList<string>();
            msgLog.AddFirst($"{context.User.GetName()} VS {user.GetName()}");

            var msg = await context.ReplyAsync(new EmbedBuilder
            {
                Description = msgLog.ListToString()
            });
            var source = new CancellationTokenSource();
            var token = source.Token;
            var battle = BattleAsync(msg, game, msgLog);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20), token);
            await Task.WhenAny(battle, timeout);
            source.Dispose();
            if(!battle.IsCompleted) battle.Dispose();

        }

        public async Task PvEBattle(HanekawaContext context)
        {

        }

        private async Task BattleAsync(IUserMessage msg, ShipGame game, LinkedList<string> combatLog)
        {
            var inProgress = true;
            while (inProgress)
            {

                inProgress = false;
            }
        }

        private void UpdateBattleLog(LinkedList<string> log, string message)
        {
            if (log.Count == 6) log.RemoveLast();
            log.AddFirst(message);
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