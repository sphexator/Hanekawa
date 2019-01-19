using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.BotGame;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Games.ShipGame.Data;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Services.Games.ShipGame
{
    public class ShipGameService : IHanaService, IRequiredService
    {
        private readonly GameStats _gameStats;

        public ShipGameService(GameStats gameStats)
        {
            _gameStats = gameStats;
            Console.WriteLine("Game service loaded");
        }

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> ExistingBattles { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ActiveBattles { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ActiveDuels { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        public event AsyncEvent<ulong> NpcKill;
        public event AsyncEvent<ulong> PvpKill;

        //TODO: Custom banner depending on monster type

        public async Task<EmbedBuilder> SearchAsync(SocketCommandContext context)
        {
            if (IsInBattle(context))
                return new EmbedBuilder().CreateDefault($"{context.User.Mention} is already in a fight",
                    Color.Red.RawValue);
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                var chance = new Random().Next(100);
                GameEnemy enemy = null;
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
                    var enemies = await db.GameEnemies.Where(x => x.Elite == false && x.Rare == false).ToListAsync();
                    enemy = enemies[new Random().Next(enemies.Count)];
                }
                else
                {
                    return new EmbedBuilder().CreateDefault(
                        $"{context.User.Mention} searched throughout the sea and found no enemy",
                        Color.Red.RawValue);
                }

                AddBattle(context, enemy);
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder {IconUrl = enemy.ImageUrl, Name = enemy.Name},
                    Description = "You encountered an enemy!\n" +
                                  $"{enemy.Name}",
                    Color = Color.Green
                };

                embed.AddField("Type", $"{(await GetClass(db, enemy.ClassId)).Name}", true);
                embed.AddField("Health", $"{await Health(db, userdata.Level, enemy)}", true);
                embed.AddField("Level", $"{userdata.Level}", true);
                return embed;
            }
        }

        private async Task<int> Health(DbService db, int level, SocketGuildUser user)
        {
            var userdata = await db.GetOrCreateUserData(user);
            return _gameStats.GetHealth(level, await GetClass(db, userdata.Class));
        }

        private async Task<int> Health(DbService db, int level, GameEnemy enemy) =>
            _gameStats.GetHealth(level, enemy, await GetClass(db, enemy.ClassId));

        private int Damage(int level) => _gameStats.GetDamage(level);

        private int Damage(int level, GameEnemy enemy) => _gameStats.GetDamage(level, enemy);

        public async Task AttackAsync(SocketCommandContext context)
        {
            if (!IsInBattle(context))
            {
                await context.ReplyAsync($"{context.User.Mention} isn't in a fight", Color.Red.RawValue);
                return;
            }

            if (ActiveBattle(context))
            {
                await context.ReplyAsync($"{context.User.Mention}, a fight is already in progress, please wait.",
                    Color.Red.RawValue);
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
                var userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                playerOne = await GetClass(db, userdata.Class);
                playerTwo = await GetClass(db, enemy.ClassId);
                playerOneHp = await Health(db, userdata.Level, context.User as SocketGuildUser);
                playerTwoHp = await Health(db, userdata.Level, enemy);
                playerOneDmg = Damage(userdata.Level);
                playerTwoDmg = Damage(userdata.Level, enemy);
                playerOneHpMax = await Health(db, userdata.Level, context.User as SocketGuildUser);
                playerTwoHpMax = await Health(db, userdata.Level, enemy);
            }

            var msglog = new LinkedList<string>();
            msglog.AddFirst($"**{(context.User as SocketGuildUser).GetName()}** VS **{enemy.Name}**");

            var img = await CreateBanner(context.User as SocketGuildUser, enemy);
            img.Seek(0, SeekOrigin.Begin);
            var embed = new EmbedBuilder().CreateDefault(UpdateCombatLog(msglog), context.Guild.Id);
            embed.AddField($"{(context.User as SocketGuildUser).GetName()}", $"{playerOneHp}/{playerOneHpMax}", true);
            embed.AddField($"{enemy.Name}", $"{playerTwoHp}/{playerTwoHpMax}", true);
            var msg = await context.Channel.SendFileAsync(img, "banner.png", null, false, embed.Build());
            var alive = true;
            await Task.Delay(2000);
            while (alive)
            {
                var usrDmg = _gameStats.CalculateDamage(playerOneDmg, playerOne, playerTwo, EnemyType.NPC);
                var npcDmg = _gameStats.CalculateDamage(playerTwoDmg, playerTwo, playerTwo, EnemyType.Player);

                playerTwoHp = playerTwoHp - usrDmg;
                if (playerTwoHp > 0)
                {
                    if (msglog.Count == 5)
                    {
                        msglog.RemoveLast();
                        msglog.AddFirst(
                            $"**{(context.User as SocketGuildUser).GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                    else
                    {
                        msglog.AddFirst(
                            $"**{(context.User as SocketGuildUser).GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                }
                else
                {
                    if (msglog.Count == 5)
                    {
                        msglog.RemoveLast();
                        msglog.AddFirst(
                            $"**{(context.User as SocketGuildUser).GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }
                    else
                    {
                        msglog.AddFirst(
                            $"**{(context.User as SocketGuildUser).GetName()}** hit **{enemy.Name}** for **{usrDmg}**");
                    }

                    // End game
                    alive = false;

                    msglog.RemoveLast();
                    msglog.AddFirst($"**{(context.User as SocketGuildUser).GetName()}** defeated **{enemy.Name}**!\n" +
                                    $"Looted **${enemy.CreditGain}** and gained **{enemy.ExpGain}** exp.");
                    RemoveBattle(context);

                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                        userdata.Credit = userdata.Credit + enemy.CreditGain;
                        userdata.Exp = userdata.Exp + enemy.ExpGain;
                        userdata.TotalExp = userdata.TotalExp + enemy.ExpGain;
                        userdata.GameKillAmount = userdata.GameKillAmount + 1;
                        await db.SaveChangesAsync();
                    }

                    embed.Color = Color.Green;
                    embed.Description = UpdateCombatLog(msglog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{(context.User as SocketGuildUser).GetName()}");
                    var enemyField = embed.Fields.First(x => x.Name == $"{enemy.Name}");
                    userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                    enemyField.Value = $"0/{playerTwoHpMax}";
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                    var _ = NpcKill(context.User.Id);
                    continue;
                }

                playerOneHp = playerOneHp - npcDmg;
                if (playerOneHp > 0 && alive)
                {
                    if (msglog.Count == 5)
                    {
                        msglog.RemoveLast();
                        msglog.AddFirst(
                            $"**{enemy.Name}** hit **{(context.User as SocketGuildUser).GetName()}** for **{npcDmg}**");
                    }
                    else
                    {
                        msglog.AddFirst(
                            $"**{enemy.Name}** hit **{(context.User as SocketGuildUser).GetName()}** for **{npcDmg}**");
                    }
                }
                else
                {
                    if (msglog.Count == 5)
                    {
                        msglog.RemoveLast();
                        msglog.AddFirst(
                            $"**{enemy.Name}** hit **{(context.User as SocketGuildUser).GetName()}** for **{npcDmg}**");
                    }
                    else
                    {
                        msglog.AddFirst(
                            $"**{enemy.Name}** hit **{(context.User as SocketGuildUser).GetName()}** for **{npcDmg}**");
                    }

                    // End game
                    alive = false;

                    msglog.RemoveLast();
                    msglog.AddFirst($"**{enemy.Name}** defeated **{(context.User as SocketGuildUser).GetName()}**!\n" +
                                    $"**{(context.User as SocketGuildUser).GetName()}** died.");
                    RemoveBattle(context);

                    embed.Color = Color.Red;
                    embed.Description = UpdateCombatLog(msglog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{(context.User as SocketGuildUser).GetName()}");
                    var enemyField = embed.Fields.First(x => x.Name == $"{enemy.Name}");
                    userField.Value = $"0/{playerOneHpMax}";
                    enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                }

                if (!alive) continue;
                {
                    embed.Description = UpdateCombatLog(msglog.Reverse());
                    var userField = embed.Fields.First(x => x.Name == $"{(context.User as SocketGuildUser).GetName()}");
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

        public async Task AttackAsync(SocketCommandContext context, SocketGuildUser playerTwoUser, int bet = 0)
        {
            if (ActiveDuel(context))
            {
                await context.ReplyAsync($"{context.User.Mention}, a fight is already in progress, please wait.",
                    Color.Red.RawValue);
                return;
            }

            UpdateDuel(context, true);
            try
            {
                Account userdata;
                Account userdata2;
                var p1Name = (context.User as SocketGuildUser).GetName();
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
                    userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                    userdata2 = await db.GetOrCreateUserData(playerTwoUser);
                    if (userdata.Credit < bet) return;
                    if (userdata2.Credit < bet) return;
                    playerOne = await GetClass(db, userdata.Class);
                    playerTwo = await GetClass(db, userdata2.Class);
                    playerOneHp = await Health(db, userdata.Level, context.User as SocketGuildUser);
                    playerTwoHp = await Health(db, userdata.Level, context.User as SocketGuildUser);
                    playerOneDmg = Damage(userdata.Level);
                    playerTwoDmg = Damage(userdata.Level);
                    playerOneHpMax = await Health(db, userdata.Level, context.User as SocketGuildUser);
                    playerTwoHpMax = await Health(db, userdata.Level, context.User as SocketGuildUser);
                }

                var msglog = new LinkedList<string>();
                msglog.AddFirst($"**{p1Name}** VS **{p2Name}**");

                var img = await CreateBanner(context.User as SocketGuildUser, playerTwoUser);
                img.Seek(0, SeekOrigin.Begin);
                var embed = new EmbedBuilder().CreateDefault(UpdateCombatLog(msglog), context.Guild.Id);

                embed.AddField($"{p1Name}", $"{playerOneHp}/{playerOneHpMax}", true);
                embed.AddField($"{p2Name}", $"{playerTwoHp}/{playerTwoHpMax}", true);
                var msg = await context.Channel.SendFileAsync(img, "banner.png", null, false, embed.Build());
                var alive = true;
                while (alive)
                {
                    var playerOneDamage =
                        _gameStats.CalculateDamage(playerOneDmg, playerOne, playerTwo, EnemyType.Player);
                    var playerTwoDamage =
                        _gameStats.CalculateDamage(playerTwoDmg, playerTwo, playerTwo, EnemyType.Player);

                    playerTwoHp = playerTwoHp - playerOneDamage;
                    if (playerTwoHp > 0)
                    {
                        if (msglog.Count == 5)
                        {
                            msglog.RemoveLast();
                            msglog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                        else
                        {
                            msglog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                    }
                    else
                    {
                        if (msglog.Count == 5)
                        {
                            msglog.RemoveLast();
                            msglog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }
                        else
                        {
                            msglog.AddFirst($"**{p1Name}** hit **{p2Name}** for **{playerOneDamage}**");
                        }

                        // End game
                        alive = false;
                        if (bet != 0)
                        {
                            msglog.AddFirst(
                                $"**{context.User.Mention}** defeated **{playerTwoUser.Mention}** and won the bet of ${bet}!");
                            winner = userdata;
                            loser = userdata2;
                        }
                        else
                        {
                            winner = userdata;
                            loser = userdata2;
                            msglog.AddFirst($"**{context.User.Mention}** defeated **{playerTwoUser.Mention}**!");
                        }

                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msglog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                        enemyField.Value = $"0/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                        continue;
                    }

                    playerOneHp = playerOneHp - playerTwoDamage;
                    if (playerOneHp > 0 && alive)
                    {
                        if (msglog.Count == 5)
                        {
                            msglog.RemoveLast();
                            msglog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                        else
                        {
                            msglog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                    }
                    else
                    {
                        if (msglog.Count == 5)
                        {
                            msglog.RemoveLast();
                            msglog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }
                        else
                        {
                            msglog.AddFirst($"**{p2Name}** hit **{p1Name}** for **{playerTwoDamage}**");
                        }

                        // End game
                        alive = false;
                        if (bet != 0)
                        {
                            msglog.AddFirst(
                                $"**{playerTwoUser.Mention}** defeated **{context.User.Mention}** and won the bet of ${bet}!");
                            loser = userdata; // Loser
                            winner = userdata2; // Winner
                        }
                        else
                        {
                            winner = userdata;
                            loser = userdata2;
                            msglog.AddFirst($"**{playerTwoUser.Mention}** defeated **{context.User.Mention}**!");
                        }

                        embed.Color = Color.Green;
                        embed.Description = UpdateCombatLog(msglog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"0/{playerOneHpMax}";
                        enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }

                    if (!alive) continue;
                    {
                        embed.Description = UpdateCombatLog(msglog.Reverse());
                        var userField = embed.Fields.First(x => x.Name == $"{p1Name}");
                        var enemyField = embed.Fields.First(x => x.Name == $"{p2Name}");
                        userField.Value = $"{playerOneHp}/{playerOneHpMax}";
                        enemyField.Value = $"{playerTwoHp}/{playerTwoHpMax}";
                        await msg.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    await Task.Delay(2000);
                }

                if (bet != 0)
                    using (var db = new DbService())
                    {
                        winner.Credit = winner.Credit + bet;
                        loser.Credit = loser.Credit - bet;
                        await db.SaveChangesAsync();
                    }

                UpdateDuel(context, false);
                Console.WriteLine("Completed duel");
                PvpKill?.Invoke(winner.UserId);
            }
            catch
            {
                UpdateDuel(context, false);
                Console.WriteLine("Duel failed");
            }
        }

        private GameEnemy GetEnemyData(SocketCommandContext context)
        {
            var battles = ExistingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryGetValue(context.User.Id, out var game);
            return game;
        }

        private static Task<GameClass> GetClass(DbService db, int id) => db.GameClasses.FindAsync(id);

        private bool ActiveBattle(SocketCommandContext context)
        {
            var gChannels = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var check = gChannels.TryGetValue(context.Channel.Id, out var value);
            if (check) return value;
            gChannels.GetOrAdd(context.Channel.Id, true);
            return false;
        }

        private void UpdateBattle(SocketCommandContext context, bool status)
        {
            var gChannels = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, status, (key, old) => old = status);
        }

        private bool ActiveDuel(SocketCommandContext context)
        {
            var gChannels = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var check = gChannels.TryGetValue(context.Channel.Id, out var value);
            if (check) return value;
            gChannels.GetOrAdd(context.Channel.Id, true);
            return false;
        }

        private void UpdateDuel(SocketCommandContext context, bool status)
        {
            var gChannels = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, status, (key, old) => old = status);
        }

        private bool IsInBattle(SocketCommandContext context)
        {
            var battles = ExistingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out var game);
            return check;
        }

        private void AddBattle(SocketCommandContext context, GameEnemy enemy)
        {
            var battles = ExistingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryAdd(context.User.Id, enemy);
        }

        private void RemoveBattle(SocketCommandContext context)
        {
            var battles = ExistingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);
        }

        public void ClearUser(SocketCommandContext context)
        {
            var battles = ExistingBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);

            var gChannels = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            gChannels.AddOrUpdate(context.Channel.Id, false, (key, old) => old = false);
        }

        private static async Task<Stream> CreateBanner(IUser userOne, GameEnemy npc)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load(@"Data\Game\background.png"))
            {
                var border = Image.Load(GetBorder());
                var aviOne = await GetAvatarAsync(userOne);
                var aviTwo = await GetAvatarAsync(npc);
                aviOne.Seek(0, SeekOrigin.Begin);
                aviTwo.Seek(0, SeekOrigin.Begin);
                var playerOne = Image.Load(aviOne);
                var playerTwo = Image.Load(aviTwo);
                img.Mutate(x => x
                    .DrawImage(GraphicsOptions.Default, playerOne, new Point(3, 92))
                    .DrawImage(GraphicsOptions.Default, playerTwo, new Point(223, 92))
                    .DrawImage(GraphicsOptions.Default, border, new Point(0, 0)));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private static async Task<Stream> CreateBanner(IUser userOne, IUser userTwo)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load(@"Data\Game\background.png"))
            {
                var border = Image.Load(GetBorder());
                var aviOne = await GetAvatarAsync(userOne);
                var aviTwo = await GetAvatarAsync(userTwo);
                aviOne.Seek(0, SeekOrigin.Begin);
                aviTwo.Seek(0, SeekOrigin.Begin);
                var playerOne = Image.Load(aviOne);
                var playerTwo = Image.Load(aviTwo);
                img.Mutate(x => x
                    .DrawImage(GraphicsOptions.Default, playerOne, new Point(3, 92))
                    .DrawImage(GraphicsOptions.Default, playerTwo, new Point(223, 92))
                    .DrawImage(GraphicsOptions.Default, border, new Point(0, 0)));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private static string GetBorder() => @"Data\Game\Border\Red-border.png";

        private static async Task<Stream> GetAvatarAsync(IUser user)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var avatar = await client.GetStreamAsync(user.GetAvatar());
                using (var img = Image.Load(avatar))
                {
                    img.Mutate(x => x.Resize(126, 126));
                    img.Save(stream, new PngEncoder());
                }
            }

            return stream;
        }

        private static async Task<Stream> GetAvatarAsync(GameEnemy npc)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var avatar = await client.GetStreamAsync(npc.ImageUrl);
                using (var img = Image.Load(avatar))
                {
                    img.Mutate(x => x.Resize(126, 126));
                    img.Save(stream, new PngEncoder());
                }
            }

            return stream;
        }

        private static string UpdateCombatLog(IEnumerable<string> log) => string.Join("\n", log);
    }
}