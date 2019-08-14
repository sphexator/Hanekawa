using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Suggestion
{
    [Name("Suggestion")]
    [Description(
        "Module for creating suggestions for a server, adds up/down votes for users to show if they think it's a good idea or not.")]
    public partial class Suggestion : InteractiveBase
    {
        [Name("Suggest")]
        [Command("suggest")]
        [Description("Sends a suggestion to the server, if they have it enabled")]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var caseId = await db.CreateSuggestion(Context.User, Context.Guild, DateTime.UtcNow);
                var embed = new EmbedBuilder().Create(suggestion, Context.Colour.Get(Context.Guild.Id));
                embed.Author = new EmbedAuthorBuilder
                    {IconUrl = Context.User.GetAvatar(), Name = Context.User.GetName()};
                embed.Footer = new EmbedFooterBuilder {Text = $"Suggestion ID: {caseId.Id}"};
                embed.Timestamp = DateTimeOffset.UtcNow;
                if (Context.Message.Attachments.Count > 0) embed.WithImageUrl(Context.Message.Attachments.First().Url);
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).ReplyAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Create("Suggestion sent!", Color.Green).Build());
                await ApplyEmotesAsync(msg, cfg);
            }
        }

        [Name("Approve Suggestion")]
        [Command("approve", "ar")]
        [Description("Approves a suggestion by its Id with a optional reason")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ApproveSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion?.MessageId == null)
                {
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason, Color.Green);
                await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        [Name("Decline Suggestion")]
        [Command("decline", "dr")]
        [Description("Decline a suggestion by its ID with a optional reason")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DeclineSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion?.MessageId == null)
                {
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason, Color.Red);
                await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        [Name("Comment Suggestion")]
        [Command("Comment", "rr")]
        [Description("Adds a comment onto a suggestion, usable by user suggesting and server admin")]
        public async Task CommentSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (!Context.User.GuildPermissions.Has(GuildPermission.ManageGuild) &&
                    Context.User.Id != suggestion.UserId) return;

                if (suggestion?.MessageId == null)
                {
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value)
                    .GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason);
                if (Context.User.Id != suggestion.UserId) await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        private async Task ApplyEmotesAsync(IUserMessage msg, SuggestionConfig cfg)
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
            for (var i = 0; i < result.Count; i++)
            {
                var x = result[i];
                await msg.AddReactionAsync(x);
            }
        }

        private async Task<string> CommentSuggestion(SocketGuildUser user, IUserMessage msg, string message,
            Color? color = null)
        {
            var embed = msg.Embeds.First().ToEmbedBuilder();
            if (color.HasValue) embed.Color = color;
            embed.AddField(user.GetName(), message);
            await msg.ModifyAsync(x => x.Embed = embed.Build());
            return embed.Description;
        }

        private async Task RespondUser(Database.Tables.Moderation.Suggestion suggestion, string sugst, string response)
        {
            try
            {
                var suggestUser = Context.Guild.GetUser(suggestion.UserId);
                if (suggestUser == null) return;
                await (await suggestUser.GetOrCreateDMChannelAsync()).ReplyAsync(
                    new EmbedBuilder().Create(
                        $"Your suggestion got a response in {Context.Guild.Name}!\n" +
                        "Suggestion:\n" +
                        $"{sugst.Truncate(300)}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{response.Truncate(1200)}", Context.Colour.Get(Context.Guild.Id)));
            }
            catch
            {
                /*IGNORE*/
            }
        }
    }
}