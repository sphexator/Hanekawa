using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Services.Level;
using Humanizer;

namespace Hanekawa.Modules.Account.Level
{
    [Group("exp")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Experience : InteractiveBase
    {
        private readonly LevelingService _levelingService;
        private readonly DbService _db;

        public Experience(LevelingService levelingService, DbService db)
        {
            _levelingService = levelingService;
            _db = db;
        }

        [Command("give")]
        [Summary("Gives a certain amount of experience to a user")]
        public async Task GiveExperience(SocketGuildUser user, uint exp)
        {
            var userData = await _db.GetOrCreateUserData(user);
            userData.Exp += exp;
            await _db.SaveChangesAsync();
            await ReplyAsync(null, false, new EmbedBuilder().Reply($"Added {exp} of exp to {user.Mention}", Color.Green.RawValue).Build());
        }

        [Command("ignore channel", RunMode = RunMode.Async)]
        [Alias("ichan")]
        [Summary("Adds or removes a channel from reduced exp pool with provided channel")]
        public async Task AddReducedExpChannel(ITextChannel channel)
        {
            var embed = await _levelingService.ReducedExpManager(null, channel);
            if (embed != null) await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("ignore category", RunMode = RunMode.Async)]
        [Alias("icat")]
        [Summary("Adds or removes a category from reduced exp pool with the provided channel(if in a category)")]
        public async Task AddReducedExpCategory(ITextChannel channel)
        {
            if (!channel.CategoryId.HasValue)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("That channel is not part of a category", Color.Red.RawValue).Build());
                return;
            }

            var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == channel.CategoryId.Value);
            if (category == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Couldn't find a category with provided argument", Color.Red.RawValue)
                        .Build());
                return;
            }

            var embed = await _levelingService.ReducedExpManager(category);
            if (embed != null) await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("ignore category", RunMode = RunMode.Async)]
        [Alias("icat")]
        [Summary("Adds or removes a category from reduced exp pool with provided category ID")]
        public async Task AddReducedExpCategory(ulong channel)
        {
            var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == channel);
            if (category == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Couldn't find a category with provided argument", Color.Red.RawValue)
                        .Build());
                return;
            }

            var embed = await _levelingService.ReducedExpManager(category);
            if (embed != null) await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("multiplier")]
        [Alias("multi")]
        [Summary("Gets the current level multiplier")]
        public async Task LevelMultiplier()
        {
            var multiplier = _levelingService.GetServerMultiplier(Context.Guild);
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply($"Current server multiplier: x{multiplier}").Build());
        }

        [Command("event", RunMode = RunMode.Async)]
        [Summary(
            "Starts a exp event with specified multiplier and duration. Auto-announced in Event channel if desired")]
        public async Task ExpEventAsync(uint multiplier, TimeSpan? duration = null)
        {
            try
            {
                if (!duration.HasValue) duration = TimeSpan.FromDays(1);
                if (duration.Value > TimeSpan.FromDays(1)) duration = TimeSpan.FromDays(1);
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply(
                        $"Wanna activate a exp event with multiplier of {multiplier} for {duration.Value.Humanize()} ({duration.Value.Humanize()}) ? (y/n)",
                        Color.Purple.RawValue).Build());
                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (response.Content.ToLower() != "y") return;

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Do you want to announce the event? (y/n)",
                        Color.Purple.RawValue).Build());
                var announceResp = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (announceResp.Content.ToLower() == "y")
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Okay, I'll let you announce it.",
                            Color.Green.RawValue).Build());
                    await _levelingService.StartExpEventAsync(Context.Guild, multiplier, duration.Value);
                }
                else
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Announcing event into designated channel.",
                            Color.Green.RawValue).Build());
                    await _levelingService.StartExpEventAsync(Context.Guild, multiplier, duration.Value, true,
                        Context.Channel as SocketTextChannel);
                }
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Exp event setup aborted.",
                        Color.Red.RawValue).Build());
            }
        }
    }
}