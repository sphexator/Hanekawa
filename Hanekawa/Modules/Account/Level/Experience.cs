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

        [Name("Give experience")]
        [Command("exp give")]
        [Summary("**Require Manage Server**\nGives a certain amount of experience to a user")]
        [Remarks("h.exp give @bob#0000 1000")]
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

        [Name("Exp ignore add")]
        [Command("exp ignore add", RunMode = RunMode.Async)]
        [Alias("eia", "exp add")]
        [Priority(1)]
        [Summary("**Require Manage Server**\nAdds a channel from reduced exp pool with provided channel")]
        [Remarks("h.eia @general")]
        public async Task AddReducedExpChannel([Remainder] ITextChannel channel) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(channel, false));

        [Name("Exp ignore add")]
        [Command("exp ignore add", RunMode = RunMode.Async)]
        [Alias("eia")]
        [Priority(2)]
        [Summary("**Require Manage Server**\nAdds a category from reduced exp pool with provided channel")]
        [Remarks("h.eia general")]
        public async Task AddReducedExpChannel([Remainder] ICategoryChannel category) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(category, false));

        [Name("Exp ignore remove")]
        [Command("exp ignore remove")]
        [Alias("eir", "exp remove")]
        [Summary("**Require Manage Server**\nRemoves a channel from reduced exp pool with provided channel")]
        [Remarks("h.eir #general")]
        public async Task RemoveReducedExpChannel([Remainder] ICategoryChannel category) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(category, true));

        [Name("Exp ignore remove")]
        [Command("exp ignore remove")]
        [Alias("eir", "exp remove")]
        [Summary("**Require Manage Server**\nRemoves a category from reduced exp pool with provided channel")]
        [Remarks("h.eir general")]
        public async Task RemoveReducedExpChannel([Remainder] ITextChannel channel) =>
            await Context.ReplyAsync(await _levelService.ReducedExpManager(channel, true));

        [Name("Exp ignore list")]
        [Command("exp ignore list")]
        [Alias("exp list")]
        [Summary("**Require Manage Server**\nList of channels and categories that got reduced exp enabled")]
        [Remarks("h.exp list")]
        public async Task ListReducedExpChannels() =>
            await PagedReplyAsync((await _levelService.ReducedExpList(Context.Guild))
                .PaginateBuilder(Context.Guild.Id, Context.Guild, "Reduced Exp channel list"));

        [Name("Exp multiplier")]
        [Command("exp multiplier")]
        [Alias("exp multi")]
        [Summary("**Require Manage Server**\nGets the current level multiplier")]
        [Remarks("h.exp multi 2")]
        public async Task LevelMultiplier() =>
            await Context.ReplyAsync($"Current server multiplier: x{_experienceHandler.GetMultiplier(Context.Guild.Id)}");

        [Name("Exp event")]
        [Command("exp event", RunMode = RunMode.Async)]
        [Summary(
            "**Require Manage Server**\nStarts a exp event with specified multiplier and duration. Auto-announced in Event channel if desired")]
        [Remarks("h.exp event 2 5h")]
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