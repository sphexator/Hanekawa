using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Suggestion")]
    [Description("Commands for suggestions")]
    [RequireBotGuildPermissions(Permission.SendMessages | Permission.EmbedLinks)]
    public class Suggestion : HanekawaCommandModule
    {
        [Name("Suggest")]
        [Description("Sends a suggestion to the server, if they have it enabled")]
        [Command("suggest", "suggestion")]
        [RequiredChannel]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var caseId = await db.CreateSuggestion(Context.Author, Context.Guild, DateTime.UtcNow);
            var builder = new LocalMessage
            {
                Content = $"New Suggestion from {Context.Author}",
                Embed = new LocalEmbed
                {
                    Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                    Author = new LocalEmbedAuthor
                        {IconUrl = Context.Author.GetAvatarUrl(), Name = Context.Author.DisplayName()},
                    Description = suggestion,
                    Footer = new LocalEmbedFooter {Text = $"Suggestion ID: {caseId.Id}"},
                    Timestamp = DateTimeOffset.UtcNow
                }
            };
            if (Context.Message.Attachments.Count > 0)
            {
                if (Context.Message.Attachments.Count == 1)
                    builder.Embed.WithImageUrl(Context.Message.Attachments[0].Url);
                else
                {
                    var attachments = new List<LocalAttachment>();
                    foreach (var x in Context.Message.Attachments)
                    {
                        var client = Context.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
                        var response = await client.GetStreamAsync(x.Url);
                        var result = response.ToEditable();
                        response.Position = 0;
                        attachments.Add(new LocalAttachment(result.ToEditable(), x.FileName));
                    }
                    builder.Attachments = attachments;
                }
            }

            var msg =
                await (Context.Guild.GetChannel(cfg.Channel.Value) as ITextChannel).SendMessageAsync(builder);
            caseId.MessageId = msg.Id;
            await db.SaveChangesAsync();
            await ReplyAndDeleteAsync(new LocalMessage().Create("Suggestion Sent!", HanaBaseColor.Ok()));
            await ApplyEmotesAsync(msg, cfg);
        }

        [Name("Approve Suggestion")]
        [Description("Approves a suggestion by its Id with a optional reason")]
        [Command("approve", "ar")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task ApproveSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
            if (suggestion?.MessageId == null)
            {
                await Reply("Couldn't find a suggestion with that id.", HanaBaseColor.Bad());
                return;
            }

            if (await Context.Bot.FetchMessageAsync(cfg.Channel.Value,
                suggestion.MessageId.Value) is not IUserMessage msg) return;
            var commentSuggestion = await CommentSuggestion(Context.Author, msg, reason, HanaBaseColor.Ok());
            await RespondUser(suggestion, commentSuggestion, reason);
        }

        [Name("Decline Suggestion")]
        [Description("Decline a suggestion by its ID with a optional reason")]
        [Command("decline", "dr")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task DeclineSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
            if (suggestion?.MessageId == null)
            {
                await Reply("Couldn't find a suggestion with that id.", Color.Red);
                return;
            }

            if (await Context.Bot.FetchMessageAsync(cfg.Channel.Value,
                suggestion.MessageId.Value) is not IUserMessage msg) return;
            var commentSuggestion = await CommentSuggestion(Context.Author, msg, reason, HanaBaseColor.Bad());
            await RespondUser(suggestion, commentSuggestion, reason);
        }

        [Name("Comment Suggestion")]
        [Description("Adds a comment onto a suggestion, usable by user suggesting and server admin")]
        [Command("Comment", "rr")]
        [RequiredChannel]
        public async Task CommentSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
            if (!Context.Author.GetGuildPermissions().Has(Permission.ManageGuild) &&
                Context.Author.Id != suggestion.UserId) return;

            if (suggestion?.MessageId == null)
            {
                await Reply("Couldn't find a suggestion with that id.", HanaBaseColor.Bad());
                return;
            }

            if (await Context.Bot.FetchMessageAsync(cfg.Channel.Value,
                suggestion.MessageId.Value) is not IUserMessage msg) return;
            var commentSuggestion = await CommentSuggestion(Context.Author, msg, reason);
            if (Context.Author.Id != suggestion.UserId) await RespondUser(suggestion, commentSuggestion, reason);
        }

        // TODO: This needs to be re-worked
        private static async Task ApplyEmotesAsync(IMessage msg, SuggestionConfig cfg)
        {
            LocalCustomEmoji iYes;
            LocalCustomEmoji iNo;
            if (LocalCustomEmoji.TryParse(cfg.EmoteYes, out var yesEmote))
            {
                iYes = yesEmote;
            }
            else
            {
                LocalCustomEmoji.TryParse("<:1yes:403870491749777411>", out var defaultYes);
                iYes = defaultYes;
            }

            if (LocalCustomEmoji.TryParse(cfg.EmoteNo, out var noEmote))
            {
                iNo = noEmote;
            }
            else
            {
                LocalCustomEmoji.TryParse("<:2no:403870492206825472>", out var defaultNo);
                iNo = defaultNo;
            }

            var result = new List<LocalCustomEmoji> {iYes, iNo};
            foreach (var x in result)
                await msg.AddReactionAsync(x);
        }

        private static async Task<string> CommentSuggestion(IMember user, IUserMessage msg, string message,
            Color? color = null)
        {
            var embed = LocalEmbed.FromEmbed(msg.Embeds[0]);
            if (color.HasValue) embed.Color = color;
            embed.AddField(user.ToString(), message);
            await msg.ModifyAsync(x => x.Embed = embed);
            return embed.Description;
        }

        private async Task RespondUser(Database.Tables.Moderation.Suggestion suggestion, string suggest,
            string response)
        {
            try
            {
                var suggestUser = await Context.Guild.GetOrFetchMemberAsync(suggestion.UserId);
                if (suggestUser == null) return;
                var builder = new LocalMessage
                {
                    Embed = new LocalEmbed
                    {
                        Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                        Description = $"Your suggestion got a response in {Context.Guild.Name}!\n" +
                                      "Suggestion:\n" +
                                      $"{suggest.Truncate(300)}\n" +
                                      $"Answer from {Context.Author}:\n" +
                                      $"{response.Truncate(1200)}"
                    }
                };
                try
                {
                    await suggestUser.SendMessageAsync(builder);
                }
                catch
                {
                    // IGnore
                }
            }
            catch
            {
                /*IGNORE*/
            }
        }

        [Name("Suggestion Admin")]
        [Group("Suggest")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public class SuggestionAdmin : Suggestion
        {
            [Name("Suggestion Channel")]
            [Description(
                "Sets a channel as channel to receive suggestions. don't mention a channel to disable suggestions.")]
            [Command("channel")]
            public async Task<DiscordCommandResult> SetSuggestionChannelAsync(ITextChannel channel = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (cfg.Channel.HasValue && channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    return Reply("Disabled suggestion channel", HanaBaseColor.Ok());
                }

                channel ??= Context.Channel;
                if (channel == null) return null;
                cfg.Channel = channel.Id;
                await db.SaveChangesAsync();
                return Reply($"All suggestions will now be sent to {channel.Mention} !",
                    HanaBaseColor.Ok());
            }

            [Name("Suggest Up Vote Emote")]
            [Description("Set custom yes emote for suggestions")]
            [Command("upvote")]
            public async Task<DiscordCommandResult> SetSuggestEmoteYesAsync(IGuildEmoji emote = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteYes = null;
                    await db.SaveChangesAsync();
                    return Reply("Set `no` reaction to default emote", HanaBaseColor.Ok());
                }

                cfg.EmoteYes = emote.GetMessageFormat();
                await db.SaveChangesAsync();
                return Reply($"Set `no` reaction to {emote}", HanaBaseColor.Ok());
            }

            [Name("Suggest Down Vote Emote")]
            [Description("Set custom no emote for suggestions")]
            [Command("downvote")]
            public async Task<DiscordCommandResult> SetSuggestEmoteNoAsync(IGuildEmoji emote = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteNo = null;
                    await db.SaveChangesAsync();
                    return Reply("Set `no` reaction to default emote", HanaBaseColor.Ok());
                }

                cfg.EmoteNo = emote.GetMessageFormat();
                await db.SaveChangesAsync();
                return Reply($"Set `no` reaction to {emote}", HanaBaseColor.Ok());
            }
        }
    }
}