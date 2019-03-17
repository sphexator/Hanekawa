using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.AutoModerator;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    [Name("Auto moderator")]
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

        [Name("Auto mod")]
        [Command("automod", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nDisplay all automod configurations")]
        [Remarks("h.automod")]
        public async Task AutoModConfig()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
                await Context.ReplyAsync(new EmbedBuilder()
                    .CreateDefault($"**Invite filter:** {cfg.FilterInvites}\n" +
                                   $"**Emote filter:** {cfg.EmoteCountFilter ?? 0}\n" +
                                   $"**Mention Filter:** {cfg.MentionCountFilter ?? 0}\n" +
                                   $"**URL filter:** {cfg.FilterUrls}\n" +
                                   $"**Average Toxicity enabled Channels:** {await db.NudeServiceChannels.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vat' for specifics)*\n" +
                                   $"**Single Toxicity enabled channels:** {await db.SingleNudeServiceChannels.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vst' for specifics)*\n" +
                                   $"**URL filter enabled channels:** {await db.UrlFilters.CountAsync(x => x.GuildId == Context.Guild.Id)} *('automod vuf' for specifics)*",
                        Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                    {
                        IconUrl = Context.Guild.IconUrl,
                        Name = $"Auto-moderator configuration for {Context.Guild.Name}"
                    }));
            }
        }

        [Name("Invite filter")]
        [Command("automod invite")]
        [Alias("automod srvfilter")]
        [Summary("**Require Manage Server**\nToggles guild invite filter, auto-deletes invites")]
        [Remarks("h.automod srvfilter")]
        public async Task InviteFilter()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
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

        [Name("Average toxicity")]
        [Command("automod avg toxicity")]
        [Alias("automod at")]
        [Summary("**Require Manage Server**\nSets avg. toxicity tolerance between 1-100, 0 to disable")]
        [Remarks("h.automod at #general 70")]
        public async Task AverageToxicityFilter(ITextChannel ch = null, int tolerance = 0)
        {
            if (tolerance < 0) return;
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

        [Name("Single toxicity")]
        [Command("automod single toxicity")]
        [Alias("automod st")]
        [Summary("**Require Manage Server**\nSets single toxicity tolerance between 1-100 with level it affects, 0 or empty to disable")]
        [Remarks("h.autmod st #general 80")]
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

        [Name("View single toxicity")]
        [Command("automod view st", RunMode = RunMode.Async)]
        [Alias("automod vst")]
        [Summary("**Require Manage Server**\nView single toxicity enabled channels with tolerance and level")]
        [Remarks("h.automod vst")]
        public async Task ViewSingleToxicityChannels()
        {
            using (var db = new DbService())
            {
                var channels = await db.SingleNudeServiceChannels.Where(x => x.GuildId == Context.Guild.Id)
                    .ToListAsync();
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

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault(Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                        {IconUrl = Context.Guild.IconUrl, Name = "Single Toxicity enabled channels"})
                    .WithFields(fields));
            }
        }

        [Name("View average toxicity")]
        [Command("automod view at", RunMode = RunMode.Async)]
        [Alias("automod vat")]
        [Summary("**Require Manage Server**\nView average toxicity enabled channels with tolerance")]
        [Remarks("h.automod vat")]
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

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault(Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                        {IconUrl = Context.Guild.IconUrl, Name = "Average Toxicity enabled channels"})
                    .WithFields(fields));
            }
        }

        [Name("Emote filter")]
        [Command("automod emote filter")]
        [Alias("automod emote")]
        [Summary("**Require Manage Server**\nSets an amount of emotes, if more it'll deleted the message, 0 or empty to disable")]
        [Remarks("h.automod emote 5")]
        public async Task EmoteFilter(int amount = 0)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
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

        [Name("Mention filter")]
        [Command("automod mention filter")]
        [Alias("automod mention")]
        [Summary("**Require Manage Server**\nSets an amount of mentions, if more it'll deleted the message, 0 or empty to disable")]
        [Remarks("h.automod mention 5")]
        public async Task MentionFilter(int amount = 0)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
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
    }
}