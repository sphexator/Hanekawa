using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Models;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Game;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService : INService
    {
        private readonly HttpClient _client;
        private readonly ExpService _exp;
        private readonly ImageGenerator _img;
        private readonly Random _random;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        public ShipGameService(HttpClient client, Random random, ImageGenerator img, ExpService exp, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _random = random;
            _img = img;
            _exp = exp;
            _provider = provider;
            _colourService = colourService;

            using (var db = new DbService())
            {
                foreach (var x in db.GameEnemies)
                    if (x.Elite) _eliteEnemies.TryAdd(x.Id, x);
                    else if (x.Rare) _rareEnemies.TryAdd(x.Id, x);
                    else _regularEnemies.TryAdd(x.Id, x);

                var cfg = db.GameConfigs.Find(1);
                if (cfg != null)
                {
                    DefaultDamage = cfg.DefaultDamage;
                    DefaultHealth = cfg.DefaultHealth;
                }
            }
        }

        public async Task<EmbedBuilder> SearchAsync(HanekawaContext context, SocketGuildUser user, DbService db)
        {
            var battles = _activeBattles.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (battles.TryGetValue(user.Id, out _))
            {
                await PvEBattle(context);
                return null;
            }
            var enemy = GetEnemy();
            if (enemy == null)
                return new EmbedBuilder().Create(
                    $"{user.GetName()} searched throughout the sea and didn't find anything", Color.Red);
            var existingBattles = _existingBattles.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            existingBattles.Set(user.Id, true, TimeSpan.FromHours(1));
            battles.Set(user.Id, enemy, TimeSpan.FromHours(1));
            var embed = new EmbedBuilder().Create("You've encountered an enemy!\n" +
                                                         $"**{enemy.Name}**", Color.Green);
            var userData = await db.GetOrCreateUserData(user);
            var enemyClass = await GetClassName(enemy.ClassId, db);
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder {Name = "Type", Value = enemyClass.Name, IsInline = true},
                new EmbedFieldBuilder
                {
                    Name = "Health", Value = GetHealth(userData.Level, enemy, enemyClass),
                    IsInline = true
                },
                new EmbedFieldBuilder {Name = "Level", Value = $"{userData.Level}", IsInline = true}
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
            var UserOneClass = await GetClassName(userDataOne.Class, db);
            var userTwoClass = await GetClassName(userDataTwo.Class, db);
            if (coinFlip == 1)
            {
                playerOne = new ShipGameUser(context.User, userDataOne.Level, UserOneClass, GetDamage(userDataOne.Level), GetHealth(userDataOne.Level, UserOneClass));
                playerTwo = new ShipGameUser(user, userDataTwo.Level, userTwoClass, GetDamage(userDataTwo.Level), GetHealth(userDataTwo.Level, userTwoClass));
            }
            else
            {
                playerOne = new ShipGameUser(user, userDataTwo.Level, userTwoClass, GetDamage(userDataTwo.Level), GetHealth(userDataTwo.Level, userTwoClass));
                playerTwo = new ShipGameUser(context.User, userDataOne.Level, UserOneClass, GetDamage(userDataOne.Level), GetHealth(userDataOne.Level, UserOneClass));
            }

            var game = new ShipGame(playerOne, playerTwo, bet);
            var msgLog = new LinkedList<string>();
            msgLog.AddFirst($"{context.User.GetName()} VS {user.GetName()}");
            var embed = new EmbedBuilder().Create(msgLog.ListToString(), _colourService.Get(context.Guild.Id));
            var stream = await _img.ShipGameBuilder(context.User.GetAvatar(), user.GetAvatar());
            stream.Position = 0;
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder { Name = playerOne.Name, Value = $"Health: {GetHealth(userDataOne.Level, UserOneClass)}" },
                new EmbedFieldBuilder { Name = playerTwo.Name, Value = $"Health: {GetHealth(userDataTwo.Level, userTwoClass)}" }
            };
            var msg = await context.Channel.SendFileAsync(stream, "banner.png", null, false, embed.Build());
            var source = new CancellationTokenSource();
            var token = source.Token;
            var battle = BattleAsync(msg, game, msgLog);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20), token);
            await Task.WhenAny(battle, timeout);
            source.Dispose();
            if (!battle.IsCompleted) battle.Dispose();

            embed = msg.Embeds.First().ToEmbedBuilder();
            var fieldOne = embed.Fields.First(x => x.Name == game.PlayerOne.Name);
            var fieldTwo = embed.Fields.First(x => x.Name == game.PlayerTwo.Name);
            embed.Color = battle.Result.IsNpc ? Color.Red : Color.Green;
            embed.Description = msgLog.ListToString();
            fieldOne.Value =
                $"Health: {game.PlayerOne.Health - game.PlayerOne.DamageTaken}/{game.PlayerOne.Health}";
            fieldTwo.Value =
                $"Health: {game.PlayerTwo.Health - game.PlayerTwo.DamageTaken}/{game.PlayerTwo.Health}";
            embed.Fields = new List<EmbedFieldBuilder>
            {
                fieldOne,
                fieldTwo
            };
            await msg.ModifyAsync(x => x.Embed = embed.Build());

            if (_existingBattles.TryGetValue(context.Guild.Id, out var existingBattles)) existingBattles.Remove(context.User.Id);
        }
        
        public async Task PvEBattle(HanekawaContext context)
        {
            using var db = new DbService();
            if (!_activeBattles.TryGetValue(context.Guild.Id, out var battles))
            {
                await context.ReplyAsync("You're currently not in combat", Color.Red);
                return;
            }

            if (!battles.TryGetValue(context.User.Id, out var battleObject))
            {
                await context.ReplyAsync("You're currently not in combat", Color.Red);
                return;
            }

            if (!(battleObject is GameEnemy enemy)) return;

            var userData = await db.GetOrCreateUserData(context.User);
            var UserOneClass = await GetClassName(userData.Class, db);
            var userTwoClass = await GetClassName(enemy.ClassId, db);
            var playerOne = new ShipGameUser(context.User, userData.Level, UserOneClass, GetDamage(userData.Level), GetHealth(userData.Level, UserOneClass));
            var playerTwo = new ShipGameUser(enemy, userData.Level, userTwoClass, GetDamage(userData.Level, enemy), GetHealth(userData.Level, enemy, userTwoClass));
            var game = new ShipGame(playerOne, playerTwo, null);
            var msgLog = new LinkedList<string>();

            msgLog.AddFirst($"{context.User.GetName()} VS {enemy.Name}");
            var embed = new EmbedBuilder().Create(msgLog.ListToString(), _colourService.Get(context.Guild.Id));
            var stream = await _img.ShipGameBuilder(context.User.GetAvatar(), enemy.ImageUrl);
            stream.Position = 0;
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder { Name = playerOne.Name, Value = $"Health: {GetHealth(userData.Level, UserOneClass)}" },
                new EmbedFieldBuilder { Name = playerTwo.Name, Value = $"Health: {GetHealth(userData.Level, enemy, userTwoClass)}" }
            };
            var msg = await context.Channel.SendFileAsync(stream, "banner.png", null, false,
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
            var fieldOne = embed.Fields.First(x => x.Name == game.PlayerOne.Name);
            var fieldTwo = embed.Fields.First(x => x.Name == game.PlayerTwo.Name);
            embed.Color = battle.Result.IsNpc ? Color.Red : Color.Green;
            embed.Description = msgLog.ListToString();
            fieldOne.Value =
                $"Health: {game.PlayerOne.Health - game.PlayerOne.DamageTaken}/{game.PlayerOne.Health}";
            fieldTwo.Value =
                $"Health: {game.PlayerTwo.Health - game.PlayerTwo.DamageTaken}/{game.PlayerTwo.Health}";
            embed.Fields = new List<EmbedFieldBuilder>
            {
                fieldOne,
                fieldTwo
            };
            await msg.ModifyAsync(x => x.Embed = embed.Build());

            battles.Remove(context.User.Id);
            if(_existingBattles.TryGetValue(context.Guild.Id, out var existingBattles)) existingBattles.Remove(context.User.Id);
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
                else
                    dmgOne = CalculateDamage(GetDamage(game.PlayerOne.Level),
                        game.PlayerOne.Class, game.PlayerTwo.Class, EnemyType.Player);
                if (game.PlayerTwo.IsNpc)
                    dmgTwo = CalculateDamage(GetDamage(game.PlayerTwo.Level, game.PlayerTwo.Enemy),
                        game.PlayerTwo.Class, game.PlayerOne.Class, EnemyType.Npc);
                else
                    dmgTwo = CalculateDamage(GetDamage(game.PlayerTwo.Level),
                        game.PlayerTwo.Class, game.PlayerOne.Class, EnemyType.Player);

                game.PlayerTwo.DamageTaken += dmgOne;
                if (game.PlayerTwo.Health - game.PlayerTwo.DamageTaken <= 0)
                {
                    UpdateBattleLog(combatLog,
                        $"{game.PlayerOne.Name} hit for {dmgOne} damage and defeated {game.PlayerTwo.Name}");
                    inProgress = false;
                    winner = game.PlayerOne;
                    game.PlayerTwo.DamageTaken = game.PlayerTwo.Health;
                    continue;
                }

                UpdateBattleLog(combatLog, $"{game.PlayerOne.Name} hit {game.PlayerTwo.Name} for {dmgOne} damage");

                game.PlayerOne.DamageTaken += dmgTwo;
                if (game.PlayerOne.Health - game.PlayerOne.DamageTaken <= 0)
                {
                    UpdateBattleLog(combatLog,
                        $"{game.PlayerTwo.Name} hit for {dmgTwo} damage and defeated {game.PlayerOne.Name}");
                    inProgress = false;
                    winner = game.PlayerTwo;
                    game.PlayerOne.DamageTaken = game.PlayerOne.Health;
                    continue;
                }

                UpdateBattleLog(combatLog, $"{game.PlayerTwo.Name} hit {game.PlayerOne.Name} for {dmgTwo} damage");

                embed = msg.Embeds.First().ToEmbedBuilder();
                var fieldOne = embed.Fields.First(x => x.Name == game.PlayerOne.Name);
                var fieldTwo = embed.Fields.First(x => x.Name == game.PlayerTwo.Name);
                embed.Description = combatLog.ListToString();
                fieldOne.Value =
                    $"Health: {game.PlayerOne.Health - game.PlayerOne.DamageTaken}/{game.PlayerOne.Health}";
                fieldTwo.Value =
                    $"Health: {game.PlayerTwo.Health - game.PlayerTwo.DamageTaken}/{game.PlayerTwo.Health}";
                embed.Fields = new List<EmbedFieldBuilder>
                {
                    fieldOne,
                    fieldTwo
                };
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
            if (chance <= 50) return _eliteEnemies[_random.Next(1, _eliteEnemies.Count)];
            if (chance <= 150) return _rareEnemies[_random.Next(1, _rareEnemies.Count)];
            if (chance > 150 && chance < 600) return _regularEnemies[_random.Next(1, _regularEnemies.Count)];
            return null;
        }
    }
}