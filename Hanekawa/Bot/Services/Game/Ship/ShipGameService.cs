using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Achievement;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Game;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService : INService
    {
        private readonly ColourService _colourService;
        private readonly ExpService _exp;
        private readonly ImageGenerator _img;
        private readonly Random _random;
        private readonly AchievementService _achievement;

        public ShipGameService(Random random, ImageGenerator img, ExpService exp, ColourService colourService, AchievementService achievement)
        {
            _random = random;
            _img = img;
            _exp = exp;
            _colourService = colourService;
            _achievement = achievement;

            using var db = new DbService();
            var cfg = db.GameConfigs.Find(1);
            if (cfg != null)
            {
                DefaultDamage = cfg.DefaultDamage;
                DefaultHealth = cfg.DefaultHealth;
            }
        }

        public async Task<EmbedBuilder> SearchAsync(HanekawaContext context)
        {
            if (IsInBattle(context))
                return new EmbedBuilder().Create($"{context.User.Mention} is already in a fight",
                    Color.Red);
            using var db = new DbService();
            var userData = await db.GetOrCreateUserData(context.User);
            var chance = _random.Next(100);
            GameEnemy enemy;
            /*
                if (chance >= 90)
                {
                    var enemies = await db.GameEnemies.Where(x => x.Rare).ToListAsync();
                    enemy = enemies[new Random().Next(enemies.Count)];
                }
                else if (chance >= 80)
                {
                    var enemies = await db.GameEnemies.Where(x => x.Elite).ToListAsync();
                    enemy = enemies[new Random().Next(enemies.Count)];
                }
                */
            if (chance >= 40)
            {
                var enemies = await db.GameEnemies.Where(x => !x.Elite && !x.Rare).ToListAsync();
                enemy = enemies[_random.Next(enemies.Count)];
            }
            else
            {
                return new EmbedBuilder().Create(
                    $"{context.User.Mention} searched throughout the sea and found no enemy",
                    Color.Red);
            }

            AddBattle(context, enemy);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder {IconUrl = enemy.ImageUrl, Name = enemy.Name},
                Description = "You encountered an enemy!\n" +
                              $"{enemy.Name}",
                Color = Color.Green
            };
            var enemyClass = await GetClass(enemy.ClassId, db);
            var health = GetHealth(userData.Level, enemy, enemyClass);
            embed.AddField("Type", $"{enemyClass.Name}", true);
            embed.AddField("Health", $"{health}", true);
            embed.AddField("Level", $"{userData.Level}", true);
            return embed;
        }

        public async Task AttackAsync(HanekawaContext context)
        {
            if (!IsInBattle(context))
            {
                await context.ReplyAsync($"{context.User.Mention} isn't in a fight", Color.Red);
                return;
            }

            if (ActiveBattle(context))
            {
                await context.ReplyAsync($"{context.User.Mention}, a fight is already in progress, please wait.",
                    Color.Red);
                return;
            }

            UpdateBattle(context, true);
            var enemy = GetEnemyData(context);
            var playerOneHp = 0;
            var playerTwoHp = 0;
            var playerOneDmg = 0;
            var playerTwoDmg = 0;
            var playerOneHpMax = 0;
            var playerTwoHpMax = 0;
            GameClass playerOne = null;
            GameClass playerTwo = null;

            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(context.User);
                playerOne = await GetClass(userData.Class, db);
                playerTwo = await GetClass(enemy.ClassId, db);
                playerOneHp = GetHealth(userData.Level, playerOne);
                playerTwoHp = GetHealth(userData.Level, enemy, playerTwo);
                playerOneDmg = GetDamage(userData.Level);
                playerTwoDmg = GetDamage(userData.Level, enemy);
                playerOneHpMax = GetHealth(userData.Level, playerOne);
                playerTwoHpMax = GetHealth(userData.Level, enemy, playerTwo);
            }

            var msgLog = new LinkedList<string>();
            msgLog.AddFirst($"**{context.User.GetName()}** VS **{enemy.Name}**");

            var img = await _img.ShipGameBuilder(context.User.GetAvatar(), enemy.ImageUrl);
            img.Seek(0, SeekOrigin.Begin);
            var embed = new EmbedBuilder().Create(UpdateCombatLog(msgLog), _colourService.Get(context.Guild.Id));
            embed.AddField($"{context.User.GetName()}", $"{playerOneHp}/{playerOneHpMax}", true);
            embed.AddField($"{enemy.Name}", $"{playerTwoHp}/{playerTwoHpMax}", true);
            var msg = await context.Channel.SendFileAsync(img, "banner.png", null, false, embed.Build());
            var alive = true;
            await Task.Delay(2000);
            while (alive)
            {
                var usrDmg = CalculateDamage(playerOneDmg, playerOne, playerTwo, EnemyType.Npc);
                var npcDmg = CalculateDamage(playerTwoDmg, playerTwo, playerTwo, EnemyType.Player);

                playerTwoHp -= usrDmg;
                if (playerTwoHp >= 0)
                {
                    if (msgLog.Count == 5)
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst(
                            $"**{context.User.GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                    else
                    {
                        msgLog.AddFirst(
                            $"**{context.User.GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                }
                else
                {
                    if (msgLog.Count == 5)
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst(
                            $"**{context.User.GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                    else
                    {
                        msgLog.AddFirst(
                            $"**{context.User.GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }

                    // End game
                    alive = false;

                    msgLog.RemoveLast();
                    msgLog.AddFirst($"**{context.User.GetName()}** defeated **{enemy.Name}**!\n" +
                                    $"Looted **${enemy.CreditGain}** and gained **{enemy.ExpGain}** exp.");
                    RemoveBattle(context);

                    using (var db = new DbService())
                    {
                        var userData = await db.GetOrCreateUserData(context.User);
                        await _exp.AddExpAsync(context.User, userData, enemy.ExpGain, enemy.CreditGain, db);
                        userData.GameKillAmount += 1;
                        await db.SaveChangesAsync();
                        await _achievement.PveKill(context.User, db);
                    }

                    embed.Color = Color.Green;
                    embed.Description = UpdateCombatLog(msgLog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{context.User.GetName()}");
                    var enemyField = embed.Fields.First(x => x.Name == $"{enemy.Name}");
                    userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                    enemyField.Value = $"0/{playerTwoHpMax}";
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                    //var _ = NpcKill(context.User.Id);
                    // TODO: Invoke this into achievement
                    continue;
                }

                playerOneHp -= npcDmg;
                if (playerOneHp >= 0 && alive)
                {
                    if (msgLog.Count == 5)
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst(
                            $"**{enemy.Name}** hit **{context.User.GetName()}** for **{npcDmg}**");
                    }
                    else
                    {
                        msgLog.AddFirst(
                            $"**{enemy.Name}** hit **{context.User.GetName()}** for **{npcDmg}**");
                    }
                }
                else
                {
                    if (msgLog.Count == 5)
                    {
                        msgLog.RemoveLast();
                        msgLog.AddFirst(
                            $"**{enemy.Name}** hit **{context.User.GetName()}** for **{npcDmg}**");
                    }
                    else
                    {
                        msgLog.AddFirst(
                            $"**{enemy.Name}** hit **{context.User.GetName()}** for **{npcDmg}**");
                    }

                    // End game
                    alive = false;

                    msgLog.RemoveLast();
                    msgLog.AddFirst($"**{enemy.Name}** defeated **{context.User.GetName()}**!\n" +
                                    $"**{context.User.GetName()}** died.");
                    RemoveBattle(context);

                    embed.Color = Color.Red;
                    embed.Description = UpdateCombatLog(msgLog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{context.User.GetName()}");
                    var enemyField = embed.Fields.First(x => x.Name == $"{enemy.Name}");
                    userField.Value = $"0/{playerOneHpMax}";
                    enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                }

                if (!alive) continue;
                {
                    embed.Description = UpdateCombatLog(msgLog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{context.User.GetName()}");
                    var enemyField = embed.Fields.First(x => x.Name == $"{enemy.Name}");
                    userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                    enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                }
                await Task.Delay(2000);
            }

            UpdateBattle(context, false);
            Console.WriteLine("Completed game");
        }

        public async Task AttackAsync(HanekawaContext context, SocketGuildUser playerTwoUser, int? bet = 0)
        {
            if (ActiveDuel(context))
            {
                await context.ReplyAsync($"{context.User.Mention}, a fight is already in progress, please wait.",
                    Color.Red);
                return;
            }

            UpdateDuel(context, true);
            try
            {
                Account userData;
                Account userData2;
                var p1Name = context.User.GetName();
                var p2Name = playerTwoUser.GetName();
                int playerOneHp;
                int playerTwoHp;
                int playerOneDmg;
                int playerTwoDmg;
                int playerOneHpMax;
                int playerTwoHpMax;
                GameClass playerOne;
                GameClass playerTwo;
                Account winner = null;
                Account loser = null;

                using (var db = new DbService())
                {
                    userData = await db.GetOrCreateUserData(context.User);
                    userData2 = await db.GetOrCreateUserData(playerTwoUser);
                    if (userData.Credit < bet) return;
                    if (userData2.Credit < bet) return;
                    playerOne = await GetClass(userData.Class, db);
                    playerTwo = await GetClass(userData2.Class, db);
                    playerOneHp = GetHealth(userData.Level, playerOne);
                    playerTwoHp = GetHealth(userData.Level, playerTwo);
                    playerOneDmg = GetDamage(userData.Level);
                    playerTwoDmg = GetDamage(userData.Level);
                    playerOneHpMax = GetHealth(userData.Level, playerOne);
                    playerTwoHpMax = GetHealth(userData.Level, playerTwo);
                }

                var msgLog = new LinkedList<string>();
                msgLog.AddFirst($"**{p1Name}** VS **{p2Name}**");

                var img = await _img.ShipGameBuilder(context.User.GetAvatar(), playerTwoUser.GetAvatar());
                img.Seek(0, SeekOrigin.Begin);
                var embed = new EmbedBuilder().Create(UpdateCombatLog(msgLog), _colourService.Get(context.Guild.Id));

                embed.AddField($"{p1Name}", $"{playerOneHp}/{playerOneHpMax}", true);
                embed.AddField($"{p2Name}", $"{playerTwoHp}/{playerTwoHpMax}", true);
                var msg = await context.Channel.SendFileAsync(img, "banner.png", null, false, embed.Build());
                var alive = true;
                while (alive)
                {
                    var playerOneDamage = CalculateDamage(playerOneDmg, playerOne, playerTwo, EnemyType.Player);
                    var playerTwoDamage = CalculateDamage(playerTwoDmg, playerTwo, playerTwo, EnemyType.Player);

                    playerTwoHp -= playerOneDamage;
                    if (playerTwoHp > 0)
                    {
                        if (msgLog.Count == 5)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                    }
                    else
                    {
                        if (msgLog.Count == 5)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }

                        // End game
                        alive = false;
                        if (bet != 0)
                        {
                            msgLog.AddFirst(
                                $"**{context.User.Mention}** defeated **{playerTwoUser.Mention}** and won the bet of ${bet}!");
                            winner = userData;
                            loser = userData2;
                        }
                        else
                        {
                            winner = userData;
                            loser = userData2;
                            msgLog.AddFirst($"**{context.User.Mention}** defeated **{playerTwoUser.Mention}**!");
                        }

                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msgLog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                        enemyField.Value = $"0/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        continue;
                    }

                    playerOneHp -= playerTwoDamage;
                    if (playerOneHp > 0 && alive)
                    {
                        if (msgLog.Count == 5)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                    }
                    else
                    {
                        if (msgLog.Count == 5)
                        {
                            msgLog.RemoveLast();
                            msgLog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                        else
                        {
                            msgLog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }

                        // End game
                        alive = false;
                        if (bet != 0)
                        {
                            msgLog
                                .AddFirst(
                                    $"**{playerTwoUser.Mention}** defeated **{context.User.Mention}** and won the bet of ${bet}!");
                            loser = userData; // Loser
                            winner = userData2; // Winner
                        }
                        else
                        {
                            winner = userData;
                            loser = userData2;
                            msgLog.AddFirst($"**{playerTwoUser.Mention}** defeated **{context.User.Mention}**!");
                        }

                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msgLog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"0/{playerOneHpMax}";
                        enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }

                    if (!alive) continue;
                    {
                        embed.Description = UpdateCombatLog(msgLog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                        enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    await Task.Delay(2000);
                }

                if (bet.HasValue && bet != 0)
                    using (var db = new DbService())
                    {
                        winner.Credit += bet.Value;
                        loser.Credit -= bet.Value;
                        await db.SaveChangesAsync();
                        await _achievement.PvpKill(winner.UserId, winner.GuildId, db);
                    }

                UpdateDuel(context, false);
                Console.WriteLine("Completed duel");
                // TODO: Invoke PvP achievement here
                //PvpKill?.Invoke(winner.UserId);
            }
            catch
            {
                UpdateDuel(context, false);
                Console.WriteLine("Duel failed");
            }
        }

        private GameEnemy GetEnemyData(HanekawaContext context)
        {
            var battles = _existingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryGetValue(context.User.Id, out var game);
            return game;
        }

        private bool ActiveBattle(HanekawaContext context)
        {
            var gChannels = _activeBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var check = gChannels.TryGetValue(context.Channel.Id, out var value);
            if (check) return value;
            gChannels.GetOrAdd(context.Channel.Id, true);
            return false;
        }

        private void UpdateBattle(HanekawaContext context, bool status)
        {
            var gChannels = _activeBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, status, (key, old) => old = status);
        }

        private bool ActiveDuel(HanekawaContext context)
        {
            var gChannels = _activeBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var check = gChannels.TryGetValue(context.Channel.Id, out var value);
            if (check) return value;
            gChannels.GetOrAdd(context.Channel.Id, true);
            return false;
        }

        private void UpdateDuel(HanekawaContext context, bool status)
        {
            var gChannels = _activeBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, status, (key, old) => old = status);
        }

        private bool IsInBattle(HanekawaContext context)
        {
            var battles = _existingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out _);
            return check;
        }

        private void AddBattle(HanekawaContext context, GameEnemy enemy)
        {
            var battles = _existingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryAdd(context.User.Id, enemy);
        }

        private void RemoveBattle(HanekawaContext context)
        {
            var battles = _existingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);
        }

        public void ClearUser(HanekawaContext context)
        {
            var battles = _existingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);

            var gChannels = _activeBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, false, (key, old) => old = false);
        }

        private string UpdateCombatLog(IEnumerable<string> log) => string.Join("\n", log);
    }
}