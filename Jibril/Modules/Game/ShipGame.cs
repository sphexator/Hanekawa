﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services.Games.ShipGame.Data;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.Games.ShipGame;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Game
{
    public class ShipGame : InteractiveBase
    {
        private readonly BaseStats _baseStats;
        private readonly ClassStats _classStats;
        private readonly EnemyStat _enemyStat;
        private readonly ShipGameService _gameService;

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> ActiveBattles { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        public ShipGame(BaseStats baseStats, ClassStats classStats, EnemyStat enemyStat, ShipGameService gameService)
        {
            _baseStats = baseStats;
            _classStats = classStats;
            _enemyStat = enemyStat;
            _gameService = gameService;
        }

        [Command("search", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 1, Measure.Seconds)]
        public async Task ShipGameSearchAsync()
        {
            var check = _gameService.isInBattle(Context);
            if (check)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"{Context.User.Mention} is already in a fight", Color.Red.RawValue)
                        .Build());
                return;
            }

            var chance = new Random().Next(100);
            if (chance >= 40)
            {
                using (var db = new DbService())
                {
                    var id = await db.GameEnemies.CountAsync();
                    var enemy = await db.GameEnemies.FindAsync((uint)id);
                    var userdata = await db.GetOrCreateUserData(Context.User);

                    _gameService.AddBattle(Context, enemy); // battles.TryAdd(Context.User.Id, enemy));
                    var embed = new EmbedBuilder
                    {
                        ImageUrl = $"https://i.imgur.com/{enemy.Image}.png",
                        Description = $"You encountered a enemy!\n" +
                                      $"{enemy.Name}",
                        Color = Color.Green
                    };
                    embed.AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Class",
                        Value = enemy.Class
                    });
                    embed.AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Health",
                        Value = $"{_enemyStat.HealthPoint((int)userdata.Level, (int)enemy.Health)}"
                    });
                    embed.AddField(new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Level",
                        Value = $"{userdata.Level}"
                    });
                    await ReplyAsync(null, false, embed.Build());
                    return;
                }
            }

            await ReplyAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} searched throughout the sea and found no enemy",
                    Color.Red.RawValue).Build());
        }

        [Command("attack", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        //[GlobalRatelimit(1, 5, Measure.Seconds)]
        public async Task AttackGameAsync()
        {
            var check = _gameService.isInBattle(Context);
            //var battles = ActiveBattles.GetOrAdd(Context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            //var check = battles.TryGetValue(Context.User.Id, out var game);
            if (!check) return;
            using (var db = new DbService())
            {
                var game = _gameService.GetEnemyData(Context);
                var userdata = await db.GetOrCreateUserData(Context.User);
                var msgLog = new LinkedList<string>();
                msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** VS **{game.Name}**");
                var userHealth = _baseStats.HealthPoint((int)userdata.Level, userdata.Class);
                var enemyHealth = _enemyStat.HealthPoint((int)userdata.Level, (int)game.Health);
                var usertotalHp = _baseStats.HealthPoint((int)userdata.Level, userdata.Class);
                var enemytotalHp = _enemyStat.HealthPoint((int)userdata.Level, (int)game.Health);
                var embed = new EmbedBuilder
                {
                    Description = UpdateCombatLog(msgLog),
                    Color = Color.Purple
                };
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"{(Context.User as SocketGuildUser).GetName()}",
                    Value = $"{userHealth}/{userHealth}"
                });
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"{game.Name}",
                    Value = $"{enemyHealth}/{enemyHealth}"
                });
                var msg = await ReplyAsync("test message", false, embed.Build());

                var alive = true;
                while (alive)
                {
                    var usrDmg = _baseStats.CriticalStrike(userdata.Class, (int)userdata.Level);
                    enemyHealth = enemyHealth - usrDmg;
                    if (enemyHealth > 0)
                    {
                        if (msgLog.Count == 3)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** hit **{game.Name}** for **{usrDmg}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** hit **{game.Name}** for **{usrDmg}**");
                        }
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{game.Name}");
                        userField.Value = $"{userHealth}/{usertotalHp}";
                        enemyField.Value = $"{enemyHealth}/{enemytotalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** defeated **{game.Name}**!\n" +
                                        $"Looted **${game.CreditGain}** and gained **{game.ExpGain}** exp.");
                        _gameService.RemoveBattle(Context);//battles.TryRemove(Context.User.Id, out var a);
                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{game.Name}");
                        userField.Value = $"{userHealth}/{usertotalHp}";
                        enemyField.Value = $"{enemyHealth}/{enemytotalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        userdata.Credit = userdata.Credit + game.CreditGain;
                        userdata.Exp = userdata.Exp + game.ExpGain;
                        userdata.GameKillAmount = userdata.GameKillAmount + 1;
                        await db.SaveChangesAsync();
                        alive = false;
                        continue;
                    }
                    await Task.Delay(1000);

                    var enmyDmg = _enemyStat.Avoidance(userdata.Class, (int)userdata.Level);
                    userHealth = userHealth - enmyDmg;
                    if (userHealth > 0)
                    {
                        if (msgLog.Count == 3)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{game.Name}** hit **{(Context.User as SocketGuildUser).GetName()}** for **{enmyDmg}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{game.Name}** hit **{(Context.User as SocketGuildUser).GetName()}** for **{enmyDmg}**");
                        }
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{game.Name}");
                        userField.Value = $"{userHealth}/{usertotalHp}";
                        enemyField.Value = $"{enemyHealth}/{enemytotalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst($"**{game.Name}** defeated **{(Context.User as SocketGuildUser).GetName()}**!\n" +
                                        $"**{(Context.User as SocketGuildUser).GetName()}** died.");
                        _gameService.RemoveBattle(Context); //battles.TryRemove(Context.User.Id, out var a);
                        embed.Color = Color.Red;
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{game.Name}");
                        userField.Value = $"{userHealth}/{usertotalHp}";
                        enemyField.Value = $"{enemyHealth}/{enemytotalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        alive = false;
                        continue;
                    }
                    await Task.Delay(1000);
                }
            }
        }

        [Command("duel", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        //[GlobalRatelimit(1, 5, Measure.Seconds)]
        public async Task AttackGameAsync(IGuildUser user, uint bet = 0)
        {
            using (var db = new DbService())
            {
                var playerOne = await db.GetOrCreateUserData(Context.User);
                var playerTwo = await db.GetOrCreateUserData(user);
                var msgLog = new LinkedList<string>();
                msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** VS **{user.GetName()}**");
                var playerOneHealth = _baseStats.HealthPoint((int)playerOne.Level, playerOne.Class);
                var playerTwoHealth = _baseStats.HealthPoint((int)playerTwo.Level, playerTwo.Class);
                var playerOnetotalHp = _baseStats.HealthPoint((int)playerOne.Level, playerOne.Class);
                var playerTwototalHp = _baseStats.HealthPoint((int)playerTwo.Level, playerTwo.Class);
                var embed = new EmbedBuilder
                {
                    Description = UpdateCombatLog(msgLog),
                    Color = Color.Purple
                };
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"{(Context.User as SocketGuildUser).GetName()}",
                    Value = $"{playerOneHealth}/{playerOnetotalHp}"
                });
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"{user.GetName()}",
                    Value = $"{playerTwoHealth}/{playerTwototalHp}"
                });
                var msg = await ReplyAsync("test message", false, embed.Build());

                var alive = true;
                while (alive)
                {
                    var usrDmg = _baseStats.CriticalStrike(playerOne.Class, (int)playerOne.Level);
                    playerTwoHealth = playerTwoHealth - usrDmg;
                    if (playerTwoHealth > 0)
                    {
                        if (msgLog.Count == 3)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** hit **{user.GetName()}** for **{usrDmg}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** hit **{user.GetName()}** for **{usrDmg}**");
                        }
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{user.GetName()}");
                        userField.Value = $"{playerOneHealth}/{playerOnetotalHp}";
                        enemyField.Value = $"{playerTwoHealth}/{playerTwototalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst($"**{(Context.User as SocketGuildUser).GetName()}** defeated **{user.GetName()}**!");
                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{user.GetName()}");
                        userField.Value = $"{playerOneHealth}/{playerOnetotalHp}";
                        enemyField.Value = $"{playerTwoHealth}/{playerTwototalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        alive = false;
                        continue;
                    }
                    await Task.Delay(1000);

                    var enmyDmg = _baseStats.CriticalStrike(playerTwo.Class, (int)playerTwo.Level);
                    playerOneHealth = playerOneHealth - enmyDmg;
                    if (playerOneHealth > 0)
                    {
                        if (msgLog.Count == 3)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{user.GetName()}** hit **{(Context.User as SocketGuildUser).GetName()}** for **{enmyDmg}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{user.GetName()}** hit **{(Context.User as SocketGuildUser).GetName()}** for **{enmyDmg}**");
                        }
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{user.GetName()}");
                        userField.Value = $"{playerOneHealth}/{playerOnetotalHp}";
                        enemyField.Value = $"{playerTwoHealth}/{playerTwototalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst($"**{user.GetName()}** defeated **{(Context.User as SocketGuildUser).GetName()}**!");
                        embed.Color = Color.Red;
                        embed.Description = UpdateCombatLog(msgLog);
                        var userField = embed.Fields.First(x => x.Name == $"{(Context.User as SocketGuildUser).GetName()}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{user.GetName()}");
                        userField.Value = $"{playerOneHealth}/{playerOnetotalHp}";
                        enemyField.Value = $"{playerTwoHealth}/{playerTwototalHp}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        alive = false;
                        continue;
                    }
                    await Task.Delay(1000);
                }
            }
        }

        private static string UpdateCombatLog(IEnumerable<string> log)
        {
            return string.Join("\n", log);
        }
    }
}