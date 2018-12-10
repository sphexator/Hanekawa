using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.AutoModerator;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Permission
{
    [Group("automoderator")]
    [Alias("automod")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class FilterPermission : InteractiveBase
    {
        private readonly ModerationService _moderation;
        private readonly NudeScoreService _nude;

        public FilterPermission(NudeScoreService nude, ModerationService moderation)
        {
            _nude = nude;
            _moderation = moderation;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Display all automod configurations")]
        public async Task AutoModConfig()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                await Context.ReplyAsync(new EmbedBuilder()
                    .CreateDefault($"**Invite filter:** {cfg.FilterInvites}\n" +
                                   $"**Emote filter:** {cfg.EmoteCountFilter ?? 0}\n" +
                                   $"**Mention Filter:** {cfg.MentionCountFilter ?? 0}\n" +
                                   $"**URL filter:** {cfg.FilterUrls}\n" +
                                   $"**Average Toxicity enabled Channels:** {await db.NudeServiceChannels.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vat' for specifics)*\n" +
                                   $"**Single Toxicity enabled channels:** {await db.SingleNudeServiceChannels.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vst' for specifics)*\n" +
                                   $"**URL filter enabled channels:** {await db.UrlFilters.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vuf' for specifics)*")
                    .WithAuthor(new EmbedAuthorBuilder
                    {
                        IconUrl = Context.Guild.IconUrl,
                        Name = $"Auto-moderator configuration for {Context.Guild.Name}"
                    }));
            }
        }

        [Command("invite")]
        [Alias("srvfilter")]
        [Summary("Toggles guild invite filter, auto-deletes invites")]
        public async Task InviteFilter()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.FilterInvites)
                {
                    cfg.FilterInvites = false;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled auto deletion and muting users posting invite links.",
                        Color.Green.RawValue);
                    return;
                }

                cfg.FilterInvites = true;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Enabled auto deletion and muting users posting invite links.",
                    Color.Green.RawValue);
            }
        }

        [Command("avg toxicity")]
        [Alias("at")]
        [Summary("Sets avg. toxicity tolerance between 1-100, 0 to disable")]
        public async Task AverageToxicityFilter(ITextChannel ch = null, uint tolerance = 0)
        {
            if (ch == null && tolerance == 0)
            {
                ch = Context.Channel as ITextChannel;
                var embed = await _nude.RemoveNudeChannel(ch);
                if (embed == null) return;
                await Context.ReplyAsync(embed);
            }
            else if (ch == null)
            {
                ch = Context.Channel as ITextChannel;
                var embed = await _nude.SetNudeChannel(ch, tolerance);
                if (embed == null) return;
                await Context.ReplyAsync(embed);
            }
            else
            {
                var embed = await _nude.SetNudeChannel(ch, tolerance);
                if (embed == null) return;
                await Context.ReplyAsync(embed);
            }
        }

        [Command("single toxicity")]
        [Alias("st")]
        [Summary("Sets single toxicity tolerance between 1-100 with level it affects, 0 or empty to disable")]
        public async Task SingleToxicityFilter(ITextChannel ch = null, int tolerance = 0, int level = 0)
        {
            if (ch == null && tolerance == 0 && level == 0)
            {
                ch = Context.Channel as ITextChannel;
                var embed = await _nude.RemoveSingleNudeChannel(ch);
                if (embed == null)
                    return;
                await Context.ReplyAsync(embed);
            }
            else if (tolerance == 0 || tolerance == 0 && level == 0)
            {
                var embed = await _nude.RemoveSingleNudeChannel(ch);
                if (embed == null)
                    return;
                await Context.ReplyAsync(embed);
            }
            else
            {
                if (tolerance < 0) return;
                if (level < 0) return;
                var embed = await _nude.SetSingleNudeChannel(ch, level, tolerance);
                if (embed == null)
                    return;
                await Context.ReplyAsync(embed);
            }
        }

        [Command("view st", RunMode = RunMode.Async)]
        [Alias("vst")]
        [Summary("View single toxicity enabled channels with tolerance and level")]
        public async Task ViewSingleToxicityChannels()
        {
            using (var db = new DbService())
            {
                var channels = await db.SingleNudeServiceChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (channels.Count == 0)
                {
                    await Context.ReplyAsync("No single toxicity channels enabled.", Color.Red.RawValue);
                    return;
                }

                var fields = new List<EmbedFieldBuilder>();
                foreach (var x in channels)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = $"{Context.Guild.GetTextChannel(x.ChannelId).Name}",
                        Value = $"Tol:{x.Tolerance} - Lvl:{x.Level}"
                    };
                    fields.Add(field);
                }

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault()
                    .WithAuthor(new EmbedAuthorBuilder { IconUrl = Context.Guild.IconUrl, Name = "Single Toxicity enabled channels" })
                    .WithFields(fields));
            }
        }

        [Command("view at", RunMode = RunMode.Async)]
        [Alias("vat")]
        [Summary("View average toxicity enabled channels with tolerance")]
        public async Task ViewAverageToxicityChannels()
        {
            using (var db = new DbService())
            {
                var channels = await db.NudeServiceChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (channels.Count == 0)
                {
                    await Context.ReplyAsync("No average toxicity channels enabled.", Color.Red.RawValue);
                    return;
                }

                var fields = new List<EmbedFieldBuilder>();
                foreach (var x in channels)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = $"{Context.Guild.GetTextChannel(x.ChannelId).Name}",
                        Value = $"Tol:{x.Tolerance}"
                    };
                    fields.Add(field);
                }

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault()
                    .WithAuthor(new EmbedAuthorBuilder { IconUrl = Context.Guild.IconUrl, Name = "Average Toxicity enabled channels" })
                    .WithFields(fields));
            }
        }

        [Command("emote filter")]
        [Alias("emote")]
        [Summary("Sets an amount of emotes, if more it'll deleted the message, 0 or empty to disable")]
        public async Task EmoteFilter(int amount = 0)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (amount > 0)
                {
                    cfg.EmoteCountFilter = amount;
                    await Context.ReplyAsync($"Set emote filter to {amount}");
                }
                else
                {
                    cfg.EmoteCountFilter = null;
                    await Context.ReplyAsync("Disabled emote filter");
                }

                await db.SaveChangesAsync();
            }
        }

        [Command("mention filter")]
        [Alias("mention")]
        [Summary("Sets an amount of mentions, if more it'll deleted the message, 0 or empty to disable")]
        public async Task MentionFilter(int amount = 0)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (amount > 0)
                {
                    cfg.MentionCountFilter = amount;
                    await Context.ReplyAsync($"Set mention filter to {amount}");
                }
                else
                {
                    cfg.MentionCountFilter = null;
                    await Context.ReplyAsync("Disabled mention filter");
                }

                await db.SaveChangesAsync();
            }
        }

        [Command("url filter", RunMode = RunMode.Async)]
        [Alias("url")]
        [Summary("Sets a channel to filter out urls ")]
        public async Task UrlFilter(ITextChannel channel = null)
        {
            if (channel == null) channel = Context.Channel as ITextChannel;
            var embed = await _moderation.UrlFilterHandler(channel);
            if (embed == null) return;
            await Context.ReplyAsync(embed);
        }

        [Command("view uf", RunMode = RunMode.Async)]
        [Alias("vuf")]
        [Summary("View channels that's enabled for URL filtering")]
        public async Task ViewUrlFilteredChannels()
        {
            using (var db = new DbService())
            {
                var channels = await db.UrlFilters.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (channels.Count == 0)
                {
                    await Context.ReplyAsync("No url filter channels enabled.", Color.Red.RawValue);
                    return;
                }

                string fields = null;
                foreach (var x in channels)
                {
                    fields += $"{Context.Guild.GetTextChannel(x.ChannelId).Mention}\n";
                }

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault(fields)
                    .WithAuthor(new EmbedAuthorBuilder { IconUrl = Context.Guild.IconUrl, Name = "URL filter enabled channels" }));
            }
        }
    }
}