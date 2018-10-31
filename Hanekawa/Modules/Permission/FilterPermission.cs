using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Services.AutoModerator;

namespace Hanekawa.Modules.Permission
{

    [Group("automoderator")]
    [Alias("automod")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class FilterPermission : InteractiveBase
    {
        private readonly NudeScoreService _nude;
        private readonly ModerationService _moderation;
        public FilterPermission(NudeScoreService nude, ModerationService moderation)
        {
            _nude = nude;
            _moderation = moderation;
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply("Disabled auto deletion and muting users posting invite links.",
                                Color.Green.RawValue).Build());
                    return;
                }

                cfg.FilterInvites = true;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Enabled auto deletion and muting users posting invite links.",
                        Color.Green.RawValue).Build());
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
                await ReplyAsync(null, false, embed.Build());
            }
            else if (ch == null)
            {
                ch = Context.Channel as ITextChannel;
                var embed = await _nude.SetNudeChannel(ch, tolerance);
                if (embed == null) return;
                await ReplyAsync(null, false, embed.Build());
            }
            else
            {
                var embed = await _nude.SetNudeChannel(ch,tolerance);
                if (embed == null) return;
                await ReplyAsync(null, false, embed.Build());
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
                await ReplyAsync(null,false,embed.Build());
            }
            else if (tolerance == 0 || (tolerance == 0 && level == 0))
            {
                var embed = await _nude.RemoveSingleNudeChannel(ch);
                if (embed == null)
                    return;
                await ReplyAsync(null,false,embed.Build());
            }
            else
            {
                var embed = await _nude.SetSingleNudeChannel(ch, level, tolerance);
                if (embed == null)
                    return;
                await ReplyAsync(null,false,embed.Build());
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
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"Set emote filter to {amount}").Build());
                }
                else
                {
                    cfg.EmoteCountFilter = null;
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("Disabled emote filter").Build());
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
                    await ReplyAsync(null,false,new EmbedBuilder().Reply($"Set mention filter to {amount}").Build());
                }
                else
                {
                    cfg.MentionCountFilter = null;
                    await ReplyAsync(null,false,new EmbedBuilder().Reply("Disabled mention filter").Build());
                }

                await db.SaveChangesAsync();
            }
        }

        [Command("url filter", RunMode = RunMode.Async)]
        [Alias("url")]
        [Summary("Sets a channel to filter out urls ")]
        public async Task UrlFilter(ITextChannel channel = null){
            if(channel == null) channel = Context.Channel as ITextChannel;
            var embed = await _moderation.UrlFilterHandler(channel);
            if(embed == null) return;
            await ReplyAsync(null, false, embed.Build());
        }
    }
}