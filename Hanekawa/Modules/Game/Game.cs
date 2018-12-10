using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Games.ShipGame;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Game
{
    public class Game : InteractiveBase
    {
        private readonly ShipGameService _gameService;

        public Game(ShipGameService gameService)
        {
            _gameService = gameService;
        }

        [Command("search", RunMode = RunMode.Async)]
        [Summary("Searches for a monster to fight")]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 1, Measure.Seconds)]
        [RequiredChannel]
        public async Task ShipGameSearchAsync()
        {
            try
            {
                await Context.ReplyAsync(await _gameService.SearchAsync(Context));
            }
            catch (Exception e)
            {
                _gameService.ClearUser(Context);
                Console.WriteLine(e);
            }
        }

        [Command("attack", RunMode = RunMode.Async)]
        [Summary("Starts a fight with a monster you've searched for")]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task AttackGameAsync()
        {
            try
            {
                await _gameService.AttackAsync(Context);
            }
            catch (Exception e)
            {
                _gameService.ClearUser(Context);
                Console.WriteLine(e);
            }
        }

        [Command("duel", RunMode = RunMode.Async)]
        [Summary("Duels a user, optinoally for money")]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task AttackGameAsync(SocketGuildUser user, uint bet = 0)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var playerOne = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var playerTwo = await db.GetOrCreateUserData(user);
                if (playerOne.Credit < bet) return;
                if (playerTwo.Credit < bet) return;

                var msg = bet == 0
                    ? $"{user.Mention}, {Context.User.Mention} has challenged you to a duel, do you accept? (y/n)"
                    : $"{user.Mention}, {Context.User.Mention} has challenged you to a duel with ${bet} at stake, do you accept? (y/n)";
                await ReplyAsync(msg);
                var status = true;
                while (status)
                    try
                    {
                        var response =
                            await NextMessageAsync(new EnsureFromUserCriterion(user.Id), TimeSpan.FromSeconds(30));
                        switch (response.Content.ToLower())
                        {
                            case "y":
                                status = false;
                                break;
                            case "n":
                                await Context.ReplyAsync("Duel cancelled.");
                                return;
                        }
                    }
                    catch
                    {
                        await Context.ReplyAsync("Duel request timed out.", Color.Red.RawValue);
                        return;
                    }

                await _gameService.AttackAsync(Context, user, bet);
            }
        }

        [Command("class", RunMode = RunMode.Async)]
        [Summary("Displays available classes and switch to them")]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task PickClassAsync()
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var classes = await db.GameClasses.Where(x => x.LevelRequirement <= (int)userdata.Level)
                    .ToListAsync();
                var result = new List<string>
                {
                    "Available classes\n" +
                    $"Your current class: **{classes.FirstOrDefault(x => x.Id == userdata.Class)?.Name}**"
                };
                foreach (var x in classes) result.Add($"{x.Id} - {x.Name} (Level:{x.LevelRequirement}");

                result.Add("Pick a class by saying the number");
                var content = string.Join("\n", result);
                await Context.ReplyAsync(content);

                try
                {
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (int.TryParse(response.Content, out var value))
                    {
                        var ass = await db.GameClasses.FindAsync(value);
                        if (ass == null)
                        {
                            await Context.ReplyAsync("Couldn't find a class with that ID.", Color.Red.RawValue);
                            return;
                        }

                        userdata.Class = value;
                        await db.SaveChangesAsync();
                        await Context.ReplyAsync($"{Context.User.Mention} changed class to {ass.Name}",
                            Color.Green.RawValue);
                        return;
                    }

                    await Context.ReplyAsync("Coundn\'t find a class with that ID.", Color.Red.RawValue);
                }
                catch
                {
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder().CreateDefault("Timed out", Color.Red.RawValue).Build(),
                        TimeSpan.FromSeconds(30));
                }
            }
        }

        [Command("class list", RunMode = RunMode.Async)]
        [Summary("Displays all classes")]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task ListClassesAsync()
        {
            using (var db = new DbService())
            {
                var classes = await db.GameClasses.ToListAsync();
                var result = new List<string> { "Classes" };
                foreach (var x in classes) result.Add($"{x.Id} - {x.Name} (Level:{x.LevelRequirement}");
                var content = string.Join("\n", result);
                await Context.ReplyAsync(content);
            }
        }

        [Command("class info", RunMode = RunMode.Async)]
        [Summary("Displays information on specific class providing ID")]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task ClassInfoAsync(int id)
        {
            using (var db = new DbService())
            {
                var classInfo = await db.GameClasses.FindAsync(id);
                if (classInfo == null)
                {
                    await Context.ReplyAsync("Couldn\'t find a class with that ID.", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync($"Information for {classInfo.Name}\n" +
                                         $"Health: {100 * classInfo.ModifierHealth}%\n" +
                                         $"Damage: {100 * classInfo.ModifierDamage}%\n" +
                                         $"Crit Chance: {classInfo.ChanceCrit}%\n" +
                                         $"Avoidance: {classInfo.ChanceAvoid}%");
            }
        }
    }
}