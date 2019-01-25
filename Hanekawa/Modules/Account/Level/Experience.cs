using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Level;
using Humanizer;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Level
{
    [Group("exp")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Experience : InteractiveBase
    {
        private readonly ExpEventHandler _expEvent;
        private readonly ExperienceHandler _experienceHandler;
        private readonly LevelService _levelService;

        public Experience(ExpEventHandler expEvent, ExperienceHandler experienceHandler, LevelService levelService)
        {
            _expEvent = expEvent;
            _experienceHandler = experienceHandler;
            _levelService = levelService;
        }

        [Command("give")]
        [Summary("Gives a certain amount of experience to a user")]
        public async Task GiveExperience(SocketGuildUser user, int exp)
        {
            if (exp <= 0) return;
            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(user);
                userData.Exp += exp;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {exp} of exp to {user.Mention}", Color.Green.RawValue);
            }
        }

        [Command("ignore add", RunMode = RunMode.Async)]
        [Alias("iadd")]
        [Priority(1)]
        [Summary("Adds a channel from reduced exp pool with provided channel")]
        public async Task AddReducedExpChannel([Remainder] ITextChannel channel) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(channel, false));

        [Command("ignore add", RunMode = RunMode.Async)]
        [Alias("iadd")]
        [Priority(2)]
        [Summary("Adds a channel from reduced exp pool with provided channel")]
        public async Task AddReducedExpChannel([Remainder] ICategoryChannel category) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(category, false));

        [Command("ignore remove")]
        [Alias("remove")]
        [Summary("Removes a channel from reduced exp pool with provided channel")]
        public async Task RemoveReducedExpChannel([Remainder] ICategoryChannel category) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(category, true));

        [Command("ignore remove")]
        [Alias("remove")]
        [Summary("Removes a channel from reduced exp pool with provided channel")]
        public async Task RemoveReducedExpChannel([Remainder] ITextChannel channel) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(channel, true));

        [Command("ignore list")]
        [Alias("list")]
        [Summary("List of channels and categories that got reduced exp enabled")]
        public async Task ListReducedExpChannels() =>
            await PagedReplyAsync((await _levelService.ReducedExpList(Context.Guild))
                .PaginateBuilder(Context.Guild.Id, Context.Guild, "Reduced Exp channel list"));

        [Command("multiplier")]
        [Alias("multi")]
        [Summary("Gets the current level multiplier")]
        public async Task LevelMultiplier() =>
            await Context.ReplyAsync($"Current server multiplier: x{_experienceHandler.GetMultiplier(Context.Guild.Id)}");

        [Command("event", RunMode = RunMode.Async)]
        [Summary(
            "Starts a exp event with specified multiplier and duration. Auto-announced in Event channel if desired")]
        public async Task ExpEventAsync(int multiplier, TimeSpan? duration = null)
        {
            if (multiplier <= 0) return;
            using (var db = new DbService())
            {
                try
                {
                    if (!duration.HasValue) duration = TimeSpan.FromDays(1);
                    if (duration.Value > TimeSpan.FromDays(2)) duration = TimeSpan.FromDays(2);
                    await Context.ReplyAsync(
                        $"Wanna activate a exp event with multiplier of {multiplier} for {duration.Value.Humanize()} ({duration.Value.Humanize()}) ? (y/n)");
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (response == null || response.Content.ToLower() != "y") return;

                    await Context.ReplyAsync("Do you want to announce the event? (y/n)");
                    var announceResp = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (announceResp == null) return;
                    if (announceResp.Content.ToLower() == "y")
                    {
                        await Context.ReplyAsync("Okay, I'll let you announce it.", Color.Green.RawValue);
                        await _expEvent.StartAsync(db, Context.Guild, multiplier, duration.Value);
                    }
                    else
                    {
                        await Context.ReplyAsync("Announcing event into designated channel.",
                            Color.Green.RawValue);
                        await _expEvent.StartAsync(db, Context.Guild, multiplier, duration.Value, true,
                            Context.Channel as SocketTextChannel);
                    }
                }
                catch
                {
                    await Context.ReplyAsync("Exp event setup aborted.", Color.Red.RawValue);
                }
            }
        }
    }
}