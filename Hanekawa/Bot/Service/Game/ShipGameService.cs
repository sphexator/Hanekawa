using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Hanekawa.Bot.Commands;
using Hanekawa.Bot.Service.Achievements;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.ImageGeneration;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.ShipGame;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Exceptions;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Game
{
    public class ShipGameService : INService
    {
        private readonly AchievementService _achievement;
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;
        private readonly ExpService _exp;
        private readonly ImageGenerationService _image;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;
        private readonly Random _random;

        public ShipGameService(Hanekawa bot, IServiceProvider provider, Random random, AchievementService achievement,
            ExpService exp, CacheService cache, ImageGenerationService image)
        {
            _bot = bot;
            _provider = provider;
            _random = random;
            _achievement = achievement;
            _exp = exp;
            _cache = cache;
            _image = image;
            _logger = LogManager.GetCurrentClassLogger();
        }

        private int DefaultHealth { get; } = 10;
        private int DefaultDamage { get; } = 1;

        public async Task<ShipGameResult> SearchAsync(HanekawaCommandContext context)
        {
            if (_cache.TryGetShipGame(context.ChannelId))
                throw new HanaCommandException("Game already in progress in this channel");
            var chance = _random.Next(100);
            if (chance < 40) return null;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(context.Author);
            var gameClass = await db.GameClasses.FindAsync(userData.Class);
            var enemies = await db.GameEnemies.Where(x => !x.Elite && !x.Rare).ToListAsync();
            var enemy = enemies[_random.Next(enemies.Count)];
            var enemyClass = await db.GameClasses.FindAsync(enemy.ClassId);
            var result = await InitializeBattleAsync(new ShipGame
            {
                PlayerOne = new ShipUser(context.Author, userData.Level, gameClass, GetDamage(userData.Level),
                    GetHealth(userData.Level, gameClass)),
                PlayerTwo = new ShipUser(enemy, userData.Level, enemyClass, GetDamage(userData.Level, enemy),
                    GetHealth(userData.Level, enemy, enemyClass)),
                Exp = enemy.ExpGain,
                Credit = enemy.CreditGain,
                Bet = null,
                Type = ShipGameType.PvE,
                Channel = context.Channel as ITextChannel
            });
            if (result.Winner.IsNpc) return result;
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(context.GuildId);
            var exp = await _exp.AddExpAsync(context.Author, userData, enemy.ExpGain, enemy.CreditGain, db,
                ExpSource.Other);
            result.Log.AddFirst($"Rewarded: {currencyCfg.ToCurrencyFormat(enemy.CreditGain)} & {exp} experience");
            var embed = LocalEmbed.FromEmbed(result.Message.Embeds[0]);
            embed.Description = UpdateCombatLog(result.Log.Reverse());
            embed.Color = HanaBaseColor.Red();
            await result.Message.ModifyAsync(x => x.Embeds = new[] {embed});
            return result;
        }

        public async Task<ShipGameResult> InitializeBattleAsync(ShipGame game)
        {
            try
            {
                _cache.AddGame(game.Channel.Id, ShipGameType.PvE);
                var result = await BattleAsync(game);
                // TODO: Game Achieve 
                // if (game.Type == ShipGameType.PvE && !result.Winner.IsNpc)
                //     _ = _achievement.GameKill(game.Channel.GuildId, result.Winner.Id, false);
                // else _ = _achievement.GameKill(game.Channel.GuildId, result.Winner.Id, true);
                _cache.RemoveGame(game.Channel.Id);
                return result;
            }
            catch (Exception e)
            {
                _cache.RemoveGame(game.Channel.Id);
                _logger.Log(LogLevel.Error, e, $"(ShipGame Service) Error while executing a PvE game - {e.Message}");
                throw new HanaCommandException("Couldn't finish the game...");
            }
        }

        private async Task<ShipGameResult> BattleAsync(ShipGame game)
        {
            var first = _random.Next(1, 3);
            var log = new LinkedList<string>();
            ShipUser winner = null;

            var attacker = first == 1 ? game.PlayerOne : game.PlayerTwo;
            var target = first == 1 ? game.PlayerTwo : game.PlayerOne;
            log.AddFirst($"**{attacker.Name}** VS. **{target.Name}**");
            var msg = await _bot.SendMessageAsync(game.Channel.Id, new LocalMessage
            {
                Attachments = new List<LocalAttachment>(new[]
                {
                    new LocalAttachment(await _image.ShipGame(game.PlayerOne.Avatar, game.PlayerTwo.Avatar), "Game.png")
                }),
                Embeds = new[]
                {
                    new LocalEmbed
                    {
                        Color = HanaBaseColor.Lime(),
                        Description = UpdateCombatLog(log.Reverse()),
                        Fields = new List<LocalEmbedField>
                        {
                            new() {Name = attacker.Name, Value = $"{attacker.Health} / {attacker.MaxHealth}"},
                            new() {Name = target.Name, Value = $"{target.Health} / {target.MaxHealth}"}
                        }
                    }
                }
            });
            while (winner == null)
            {
                var result = Round(attacker, target, game.Type, log);
                if (result != null)
                {
                    winner = result;
                    continue;
                }

                var temp = attacker;
                attacker = target;
                target = temp;

                var embed = LocalEmbed.FromEmbed(msg.Embeds[0]);
                embed.Description = UpdateCombatLog(log.Reverse());
                var attackField = embed.Fields.FirstOrDefault(x => x.Name == attacker.Name);

                if (attackField != null) attackField.Value = $"{attacker.Health} / {attacker.MaxHealth}";
                var targetField = embed.Fields.FirstOrDefault(x => x.Name == target.Name);
                if (targetField != null) targetField.Value = $"{target.Health} / {target.MaxHealth}";

                await msg.ModifyAsync(x => x.Embeds = new[] {embed});
                await Task.Delay(2000);
            }

            log.AddFirst($"**{attacker.Name}** defeated **{target.Name}**!");
            return new ShipGameResult(game, winner, msg);
        }

        private ShipUser Round(ShipUser player, ShipUser target, ShipGameType gameType, LinkedList<string> msgLog)
        {
            var dmg = CalculateDamage(player.Damage, player.Class, target.Class, gameType);
            target.Health -= dmg;

            if (msgLog.Count == 5) msgLog.RemoveLast();
            msgLog.AddFirst($"**{player.Name}** hit **{target.Name}** for **{dmg}**");

            return target.Health >= 0 ? player : null;
        }

        private int CalculateDamage(int damage, GameClass attackerClass, GameClass enemyClass, ShipGameType type)
        {
            var avoidance = _random.Next(100);
            var criticalChance = _random.Next(100);
            if (type == ShipGameType.PvP)
                if (avoidance <= enemyClass.ChanceAvoid)
                    return 0;
            if (criticalChance <= attackerClass.ChanceCrit)
                damage = Convert.ToInt32(damage * attackerClass.ModifierCriticalChance);
            var lowDmg = damage / 2;
            if (lowDmg <= 0) lowDmg = 5;
            var highDmg = damage * 2;
            if (lowDmg >= highDmg) highDmg = lowDmg + 10;
            return _random.Next(lowDmg, highDmg);
        }

        private int GetDamage(int level)
        {
            return DefaultDamage * level;
        }

        private int GetDamage(int level, GameEnemy enemy)
        {
            return (DefaultDamage + enemy.Damage) * level;
        }

        private int GetHealth(int level, GameClass ass)
        {
            return Convert.ToInt32(Math.Round(DefaultHealth * level * ass.ModifierHealth));
        }

        private int GetHealth(int level, GameEnemy enemyData, GameClass enemyClass)
        {
            return Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                              enemyClass.ModifierHealth));
        }

        private static string UpdateCombatLog(IEnumerable<string> log)
        {
            return string.Join("\n", log);
        }
    }
}