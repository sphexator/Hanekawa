using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Suggestion
{
    public class Suggestion : InteractiveBase
    {
        [Name("Suggest")]
        [Command("Suggest", RunMode = RunMode.Async)]
        [Summary("Sends a suggestion")]
        [Remarks("h.suggest this is a suggestion")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.AddReactions)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [Ratelimit(1, 30, Measure.Seconds)]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            using (var db = new DbService())
            {
                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch
                { /* IGNORE */
                }

                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var caseId = await db.CreateSuggestion(Context.User, Context.Guild, DateTime.UtcNow);
                var embed = new EmbedBuilder().CreateDefault(suggestion, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                    {
                        IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                        Name = (Context.User as SocketGuildUser).GetName()
                    })
                    .WithFooter(new EmbedFooterBuilder {Text = $"Suggestion ID: {caseId.Id}"})
                    .WithTimestamp(DateTimeOffset.UtcNow);
                if (Context.Message.Attachments.Count > 0) embed.WithImageUrl(Context.Message.Attachments.First().Url);
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).ReplyAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Suggestion sent!", Color.Green.RawValue).Build());
                await SetEmotesAsync(msg, cfg);
            }
        }

        [Name("Approve suggestion")]
        [Command("approve", RunMode = RunMode.Async)]
        [Alias("ar")]
        [Summary("Approves a suggestion")]
        [Remarks("h.approve 5 yes, this suggestion we will approve")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ApproveAsync(int id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue && !suggestion.MessageId.HasValue) return;
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                embed.Color = Color.Green;
                var field = new EmbedFieldBuilder
                {
                    Name = (Context.User as SocketGuildUser).GetName(),
                    Value = response
                };
                embed.AddField(field);
                try
                {
                    var suggestUser = Context.Guild.GetUser(suggestion.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).ReplyAsync(
                        new EmbedBuilder().CreateDefault(
                            "Your suggestion got a response!\n" +
                            "Suggestion:\n" +
                            $"{embed.Description}\n" +
                            $"Answer from {Context.User}:\n" +
                            $"{response}", Context.Guild.Id));
                }
                catch
                {
                    /*IGNORE*/
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Name("Decline suggestion")]
        [Command("deny", RunMode = RunMode.Async)]
        [Alias("dr")]
        [Summary("Declines a suggestion")]
        [Remarks("h.deny 5 no, this suggestion has been declined")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DenyAsync(int id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                embed.Color = Color.Red;
                var field = new EmbedFieldBuilder
                {
                    Name = (Context.User as SocketGuildUser).GetName(),
                    Value = response
                };
                embed.AddField(field);
                try
                {
                    var suggestUser = Context.Guild.GetUser(suggestion.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).ReplyAsync(
                        "Your suggestion got a response!\n" +
                        "Suggestion:\n" +
                        $"{embed.Description}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{response}", Context.Guild.Id);
                }
                catch
                {
                    /*IGNORE*/
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Name("Comment suggestion")]
        [Command("comment", RunMode = RunMode.Async)]
        [Alias("rr")]
        [Summary("Comments a suggestion")]
        [Remarks("h.comment 5 yes this is a response")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task CommentAsync(int id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                var field = new EmbedFieldBuilder
                {
                    Name = (Context.User as SocketGuildUser).GetName(),
                    Value = response
                };
                embed.AddField(field);
                try
                {
                    var suggestUser = Context.Guild.GetUser(suggestion.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(
                        "Your suggestion got a response!\n" +
                        "Suggestion:\n" +
                        $"{embed.Description}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{response}");
                }
                catch
                {
                    /*IGNORE*/
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Name("Suggestion channel")]
        [Command("suggestion set channel", RunMode = RunMode.Async)]
        [Alias("ssc", "sschannel", "ss channel")]
        [Summary("Sets a channel as channel to receive reports. don't mention a channel to disable reports.")]
        [Remarks("h.ss channel #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (cfg.Channel.HasValue && channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled suggestion channel", Color.Green.RawValue);
                    return;
                }
                if(channel == null) channel = Context.Channel as ITextChannel;
                cfg.Channel = channel.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"All suggestions will now be sent to {channel.Mention} !",
                    Color.Green.RawValue);
            }
        }

        [Name("Suggestion down vote emote")]
        [Command("suggestion set no", RunMode = RunMode.Async)]
        [Alias("ssn", "ssno", "ss no")]
        [Summary("Set custom no emote for suggestions")]
        [Remarks("h.ss no :noEmote:")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionNoEmoteAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteNo = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green.RawValue);
                    return;
                }

                cfg.EmoteNo = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Suggestion up vote emote")]
        [Command("suggestion set yes", RunMode = RunMode.Async)]
        [Alias("ssy", "ssyes", "ss yes")]
        [Summary("Set custom yes emote for suggestions")]
        [Remarks("h.ss yes :yesEmote:")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionYesEmoteAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteYes = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green.RawValue);
                    return;
                }

                cfg.EmoteYes = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
            }
        }

        private static string ParseEmoteString(Emote emote) =>
            emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";

        private static async Task SetEmotesAsync(IUserMessage msg, SuggestionConfig cfg)
        {
            IEmote iYes;
            IEmote iNo;
            if (Emote.TryParse(cfg.EmoteYes, out var yesEmote))
            {
                iYes = yesEmote;
            }
            else
            {
                Emote.TryParse("<:1yes:403870491749777411>", out var defaultYes);
                iYes = defaultYes;
            }

            if (Emote.TryParse(cfg.EmoteNo, out var noEmote))
            {
                iNo = noEmote;
            }
            else
            {
                Emote.TryParse("<:2no:403870492206825472>", out var defaultNo);
                iNo = defaultNo;
            }

            var result = new List<IEmote> {iYes, iNo};
            foreach (var x in result)
            {
                await msg.AddReactionAsync(x);
                await Task.Delay(260);
            }
        }
    }
}