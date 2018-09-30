using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;

namespace Hanekawa.Modules.Suggestion
{
    public class Suggestion : InteractiveBase
    {
        [Command("Suggest", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.SuggestionChannel.HasValue) return;
                var caseId = await db.CreateSuggestion(Context.User, Context.Guild, DateTime.UtcNow);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                    Name = (Context.User as SocketGuildUser).GetName()
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = $"Suggestion ID: {caseId.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Timestamp = DateTimeOffset.UtcNow,
                    Color = Color.Purple,
                    Description = suggestion,
                    Footer = footer
                };
                var msg = await Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value).SendEmbedAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply("Suggestion sent!", Color.Green.RawValue).Build());
                await SetEmotesAsync(msg, cfg);
            }
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Alias("ar")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ApproveAsync(uint id, [Remainder] string response)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value)
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
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(null, false, new EmbedBuilder().Reply(
                        $"Your suggestion got a response!\n" +
                        $"Suggestion:\n" +
                        $"{embed.Description}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{response}").Build());
                }
                catch
                {
                    /*IGNORE*/
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Command("deny", RunMode = RunMode.Async)]
        [Alias("dr")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DenyAsync(uint id, [Remainder] string response)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value)
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
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(
                        $"Your suggestion got a response!\n" +
                        $"Suggestion:\n" +
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

        [Command("comment", RunMode = RunMode.Async)]
        [Alias("rr")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task CommentAsync(uint id, [Remainder] string response)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value)
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
                        $"Your suggestion got a response!\n" +
                        $"Suggestion:\n" +
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

        [Command("suggestion set channel", RunMode = RunMode.Async)]
        [Alias("ssc", "sschannel", "ss channel")]
        [Summary("Sets a channel as channel to recieve reports. don't mention a channel to disable reports.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.SuggestionChannel.HasValue && channel == null)
                {
                    cfg.SuggestionChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled suggestion channel", Color.Green.RawValue).Build());
                    return;
                }

                cfg.SuggestionChannel = channel.Id;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"All suggestions will now be sent to {channel.Mention} !",
                        Color.Green.RawValue).Build());
            }
        }

        [Command("suggestion set no", RunMode = RunMode.Async)]
        [Alias("ssn", "ssno", "ss no")]
        [Summary("Set custom no emote for suggestions")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionNoEmoteAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (emote == null)
                {
                    cfg.SuggestionEmoteNo = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Set `no` reaction to default emote", Color.Green.RawValue)
                            .Build());
                    return;
                }

                cfg.SuggestionEmoteNo = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set `no` reaction to {emote}", Color.Green.RawValue).Build());
            }
        }

        [Command("suggestion set yes", RunMode = RunMode.Async)]
        [Alias("ssy", "ssyes", "ss yes")]
        [Summary("Set custom yes emote for suggestions")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetSuggestionYesEmoteAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (emote == null)
                {
                    cfg.SuggestionEmoteYes = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Set `no` reaction to default emote", Color.Green.RawValue)
                            .Build());
                    return;
                }

                cfg.SuggestionEmoteYes = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set `no` reaction to {emote}", Color.Green.RawValue).Build());
            }
        }

        private static string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";
        }

        private static async Task SetEmotesAsync(IUserMessage msg, GuildConfig cfg)
        {
            IEmote iYes;
            IEmote iNo;
            if (Emote.TryParse(cfg.SuggestionEmoteYes, out var yesEmote)) iYes = yesEmote;
            else
            {
                Emote.TryParse("<:1yes:403870491749777411>", out var defaultYes);
                iYes = defaultYes;
            }

            if (Emote.TryParse(cfg.SuggestionEmoteNo, out var noEmote)) iNo = noEmote;
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