﻿using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Preconditions;
using Jibril.Services.Games.ShipGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Jibril.Extensions;
using Jibril.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Game
{
    public class ShipGame : InteractiveBase
    {
        private readonly ShipGameService _gameService;

        public ShipGame(ShipGameService gameService)
        {
            _gameService = gameService;
        }

        [Command("search", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 1, Measure.Seconds)]
        [RequiredChannel(346429281314013184)]
        public async Task ShipGameSearchAsync()
        {
            try
            {
                await ReplyAsync(null, false, (await _gameService.SearchAsync(Context)).Build());
            }
            catch (Exception e)
            {
                _gameService.ClearUser(Context);
                Console.WriteLine(e);
            }
        }

        [Command("attack", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(346429281314013184)]
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
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(346429281314013184)]
        public async Task AttackGameAsync(SocketGuildUser user, uint bet = 0)
        {
            if (user == Context.User) return;
            var msg = bet == 0
                ? $"{user.Mention}, {Context.User.Mention} has challenged you to a duel, do you accept? (y/n)"
                : $"{user.Mention}, {Context.User.Mention} has challenged you to a duel with ${bet} at stake, do you accept? (y/n)";
            await ReplyAsync(msg);
            var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id), TimeSpan.FromSeconds(30));
            if (response.Content.ToLower() != "y")
            {
                await ReplyAsync("Duel cancelled");
                return;
            }
            await _gameService.AttackAsync(Context, user, bet);
        }

        [Group("class")]
        public class ShipGameClass : InteractiveBase
        {
            private readonly ShipGameService _gameService;
            public ShipGameClass(ShipGameService gameService)
            {
                _gameService = gameService;
            }

            [Command(RunMode = RunMode.Async)]
            [RequireContext(ContextType.Guild)]
            [RequiredChannel(346429281314013184)]
            public async Task PickClassAsync()
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var classes = await db.GameClasses.Where(x => x.LevelRequirement <= (int)userdata.Level)
                        .ToListAsync();
                    var result = new List<string> {"Available classes\n" +
                                                   $"You're currently {classes.FirstOrDefault(x => x.Id == userdata.Class).Name}"};
                    foreach (var x in classes)
                    {
                        result.Add($"{x.Id} - {x.Name} (Level:{x.LevelRequirement}");
                    }

                    result.Add("Pick a class by saying the number");
                    var content = string.Join("\n", result);
                    await ReplyAsync(null, false, new EmbedBuilder().Reply(content).Build());
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    int.TryParse(response.Content, out var value);
                    var ass = await db.GameClasses.FindAsync(value);
                    if (ass == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"Coundn't find a class with that ID.", Color.Red.RawValue)
                                .Build());
                        return;
                    }

                    userdata.Class = value;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"{Context.User.Mention} changed class to {ass.Name}", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("list", RunMode = RunMode.Async)]
            [RequireContext(ContextType.Guild)]
            [RequiredChannel(346429281314013184)]
            public async Task ListClassesAsync()
            {
                using (var db = new DbService())
                {
                    var classes = await db.GameClasses.ToListAsync();
                    var result = new List<string> {"Classes"};
                    foreach (var x in classes)
                    {
                        result.Add($"{x.Id} - {x.Name} (Level:{x.LevelRequirement}");
                    }
                    var content = string.Join("\n", result);
                    await ReplyAsync(null, false, new EmbedBuilder().Reply(content).Build());
                }
            }

            [Command("info", RunMode = RunMode.Async)]
            [RequireContext(ContextType.Guild)]
            [RequiredChannel(346429281314013184)]
            public async Task ClassInfoAsync(int id)
            {
                using (var db = new DbService())
                {
                    var classInfo = await db.GameClasses.FindAsync(id);
                    if (classInfo == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"Couldn't find a class with that ID.", Color.Red.RawValue)
                                .Build());
                        return;
                    }

                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Information for {classInfo.Name}\n" +
                                                 $"Health: {100 * classInfo.ModifierHealth}%\n" +
                                                 $"Damage: {100 * classInfo.ModifierHealth}%\n" +
                                                 $"Crit Chance: {classInfo.ChanceCrit}%\n" +
                                                 $"Avoidance: {classInfo.ChanceAvoid}%").Build());
                }
            }
        }
    }
}