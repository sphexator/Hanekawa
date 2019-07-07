using Hanekawa.Bot.Services.ImageGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Models;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Game;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService : INService
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly ImageGenerator _img;
        private readonly ExpService _exp;

        public ShipGameService(HttpClient client, Random random, ImageGenerator img, ExpService exp)
        {
            _client = client;
            _random = random;
            _img = img;
            _exp = exp;

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
                playerOne = new ShipGameUser(context.User, userDataOne.Level, await GetClassName(userDataOne.Class, db));
                playerTwo = new ShipGameUser(user, userDataTwo.Level, await GetClassName(userDataTwo.Class, db));
            }
            else
            {
                playerOne = new ShipGameUser(user, userDataTwo.Level, await GetClassName(userDataTwo.Class, db));
                playerTwo = new ShipGameUser(context.User, userDataOne.Level, await GetClassName(userDataOne.Class, db));
            }
            var game = new ShipGame(playerOne, playerTwo, bet);
            var msgLog = new LinkedList<string>();
            msgLog.AddFirst($"{context.User.GetName()} VS {user.GetName()}");
            var embed = new EmbedBuilder().CreateDefault(msgLog.ListToString(), context.Guild.Id);
            embed.ImageUrl = "attachment://banner.png";
            var msg = await context.Channel.SendFileAsync(new MemoryStream(), "banner.png", null, false,
                embed.Build());
            var source = new CancellationTokenSource();
            var token = source.Token;
            var battle = BattleAsync(msg, game, msgLog);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20), token);
            await Task.WhenAny(battle, timeout);
            source.Dispose();
            if(!battle.IsCompleted) battle.Dispose();

            embed = msg.Embeds.First().ToEmbedBuilder();
            embed.Color = Color.Green;
            embed.Description = msgLog.ListToString();
            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }

        public async Task PvEBattle(HanekawaContext context)
        {
            using var db = new DbService();
            if(!_activeBattles.TryGetValue(context.Guild.Id, out var battles))
            {
                await context.ReplyAsync("You're currently not in combat", Color.Red.RawValue);
                return;
            }

            if (!battles.TryGetValue(context.User.Id, out var battleObject))
            {
                await context.ReplyAsync("You're currently not in combat", Color.Red.RawValue);
                return;
            }
            if (!(battleObject is GameEnemy enemy)) return;

            var userData = await db.GetOrCreateUserData(context.User);
            var playerOne = new ShipGameUser(context.User, userData.Level, await GetClassName(userData.Class, db));
            var playerTwo = new ShipGameUser(enemy, userData.Level, await GetClassName(enemy.ClassId, db));
            var game = new ShipGame(playerOne, playerTwo, null);
            var msgLog = new LinkedList<string>();
            
            msgLog.AddFirst($"{context.User.GetName()} VS {enemy.Name}");
            var embed = new EmbedBuilder().CreateDefault(msgLog.ListToString(), context.Guild.Id);
            embed.ImageUrl = "attachment://banner.png";
            var msg = await context.Channel.SendFileAsync(new MemoryStream(), "banner.png", null, false,
                embed.Build());
            var source = new CancellationTokenSource();
            var token = source.Token;
            var battle = BattleAsync(msg, game, msgLog);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20), token);
            await Task.WhenAny(battle, timeout);
            source.Dispose();
            if (!battle.IsCompleted) battle.Dispose();
            if (!battle.Result.IsNpc && battle.IsCompleted)
                await _exp.AddExpAsync(context.User, userData, enemy.ExpGain, enemy.CreditGain, db);

            embed = msg.Embeds.First().ToEmbedBuilder();
            embed.Color = battle.Result.IsNpc ? Color.Red : Color.Green;
            embed.Description = msgLog.ListToString();
            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }

        private async Task<ShipGameUser> BattleAsync(IUserMessage msg, ShipGame game, LinkedList<string> combatLog)
        {
            var inProgress = true;
            ShipGameUser winner = null;
            EmbedBuilder embed;
            while (inProgress)
            {
                int dmgOne;
                int dmgTwo;
                if (game.PlayerOne.IsNpc)
                    dmgOne = CalculateDamage(GetDamage(game.PlayerOne.Level, game.PlayerOne.Enemy),
                        game.PlayerOne.Class, game.PlayerTwo.Class, EnemyType.Npc);
                else dmgOne = CalculateDamage(GetDamage(game.PlayerOne.Level),
                    game.PlayerOne.Class, game.PlayerTwo.Class, EnemyType.Player);
                if(game.PlayerTwo.IsNpc)
                    dmgTwo = CalculateDamage(GetDamage(game.PlayerTwo.Level, game.PlayerTwo.Enemy),
                        game.PlayerTwo.Class, game.PlayerOne.Class, EnemyType.Npc);
                else dmgTwo = CalculateDamage(GetDamage(game.PlayerTwo.Level),
                    game.PlayerTwo.Class, game.PlayerOne.Class, EnemyType.Player);

                game.PlayerTwo.DamageTaken += dmgOne;
                if (game.PlayerTwo.Health - game.PlayerOne.DamageTaken <= 0)
                {
                    UpdateBattleLog(combatLog, $"{game.PlayerOne.Name} hit for {dmgOne} damage and defeated {game.PlayerTwo.Name}");
                    inProgress = false;
                    winner = game.PlayerOne;
                    continue;
                }
                UpdateBattleLog(combatLog, $"{game.PlayerOne.Name} hit {game.PlayerTwo.Name} for {dmgOne} damage");

                game.PlayerOne.DamageTaken += dmgTwo;
                if (game.PlayerOne.Health - game.PlayerTwo.DamageTaken <= 0)
                {
                    UpdateBattleLog(combatLog, $"{game.PlayerTwo.Name} hit for {dmgTwo} damage and defeated {game.PlayerOne.Name}");
                    inProgress = false;
                    winner = game.PlayerTwo;                    continue;
                }
                UpdateBattleLog(combatLog, $"{game.PlayerTwo.Name} hit {game.PlayerOne.Name} for {dmgTwo} damage");

                embed = msg.Embeds.First().ToEmbedBuilder();
                embed.Description = combatLog.ListToString();
                await msg.ModifyAsync(x => x.Embed = embed.Build());
                await Task.Delay(2000);
            }

            return winner;
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