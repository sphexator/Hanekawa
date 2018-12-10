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
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Level
{
    [Group("exp")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Experience : InteractiveBase
    {
        private readonly LevelingService _levelingService;

        public Experience(LevelingService levelingService)
        {
            _levelingService = levelingService;
        }

        [Command("give")]
        [Summary("Gives a certain amount of experience to a user")]
        public async Task GiveExperience(SocketGuildUser user, uint exp)
        {
            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(user);
                userData.Exp += exp;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {exp} of exp to {user.Mention}", Color.Green.RawValue);
            }
        }

        [Command("ignore channel", RunMode = RunMode.Async)]
        [Alias("ichan")]
        [Summary("Adds or removes a channel from reduced exp pool with provided channel")]
        public async Task AddReducedExpChannel(ITextChannel channel)
        {
            using (var db = new DbService())
            {
                var embed = await _levelingService.ReducedExpManager(db, null, channel);
                if (embed != null) await Context.ReplyAsync(embed);
            }
        }

        [Command("ignore category", RunMode = RunMode.Async)]
        [Alias("icat")]
        [Summary("Adds or removes a category from reduced exp pool with the provided channel(if in a category)")]
        public async Task AddReducedExpCategory(ITextChannel channel)
        {
            using (var db = new DbService())
            {
                if (!channel.CategoryId.HasValue)
                {
                    await Context.ReplyAsync("That channel is not part of a category", Color.Red.RawValue);
                    return;
                }

                var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == channel.CategoryId.Value);
                if (category == null)
                {
                    await Context.ReplyAsync("Couldn't find a category with provided argument", Color.Red.RawValue);
                    return;
                }

                var embed = await _levelingService.ReducedExpManager(db, category);
                if (embed != null) await Context.ReplyAsync(embed);
            }
        }

        [Command("ignore category", RunMode = RunMode.Async)]
        [Alias("icat")]
        [Summary("Adds or removes a category from reduced exp pool with provided category ID")]
        public async Task AddReducedExpCategory(ulong channel)
        {
            var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == channel);
            if (category == null)
            {
                await Context.ReplyAsync("Couldn't find a category with provided argument", Color.Red.RawValue);
                return;
            }

            using (var db = new DbService())
            {
                var embed = await _levelingService.ReducedExpManager(db, category);
                if (embed != null) await Context.ReplyAsync(embed);
            }
        }

        [Command("multiplier")]
        [Alias("multi")]
        [Summary("Gets the current level multiplier")]
        public async Task LevelMultiplier()
        {
            var multiplier = _levelingService.GetServerMultiplier(Context.Guild);
            await Context.ReplyAsync($"Current server multiplier: x{multiplier}");
        }

        [Command("event", RunMode = RunMode.Async)]
        [Summary(
            "Starts a exp event with specified multiplier and duration. Auto-announced in Event channel if desired")]
        public async Task ExpEventAsync(uint multiplier, TimeSpan? duration = null)
        {
            using (var db = new DbService())
            {
                try
                {
                    if (!duration.HasValue) duration = TimeSpan.FromDays(1);
                    if (duration.Value > TimeSpan.FromDays(1)) duration = TimeSpan.FromDays(1);
                    await Context.ReplyAsync(
                        $"Wanna activate a exp event with multiplier of {multiplier} for {duration.Value.Humanize()} ({duration.Value.Humanize()}) ? (y/n)");
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (response.Content.ToLower() != "y") return;

                    await Context.ReplyAsync("Do you want to announce the event? (y/n)");
                    var announceResp = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (announceResp.Content.ToLower() == "y")
                    {
                        await Context.ReplyAsync("Okay, I'll let you announce it.", Color.Green.RawValue);
                        await _levelingService.StartExpEventAsync(db, Context.Guild, multiplier, duration.Value);
                    }
                    else
                    {
                        await Context.ReplyAsync("Announcing event into designated channel.",
                            Color.Green.RawValue);
                        await _levelingService.StartExpEventAsync(db, Context.Guild, multiplier, duration.Value, true,
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