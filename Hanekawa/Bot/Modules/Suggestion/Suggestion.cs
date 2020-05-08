﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Suggestion
{
    [Name("Suggestion")]
    [Description(
        "Module for creating suggestions for a server, adds up/down votes for users to show if they think it's a good idea or not.")]
    public partial class Suggestion : HanekawaModule
    {
        [Name("Suggest")]
        [Command("suggest")]
        [Description("Sends a suggestion to the server, if they have it enabled")]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            await Context.Message.TryDeleteMessageAsync();
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var caseId = await db.CreateSuggestion(Context.User, Context.Guild, DateTime.UtcNow);
            var embed = new LocalEmbedBuilder().Create(suggestion, Context.Colour.Get(Context.Guild.Id.RawValue));
            embed.Author = new LocalEmbedAuthorBuilder
                {IconUrl = Context.User.GetAvatarUrl(), Name = Context.Member.DisplayName};
            embed.Footer = new LocalEmbedFooterBuilder {Text = $"Suggestion ID: {caseId.Id}"};
            embed.Timestamp = DateTimeOffset.UtcNow;
            if (Context.Message.Attachments.Count > 0) embed.WithImageUrl(Context.Message.Attachments.First().Url);
            var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).ReplyAsync(embed);
            caseId.MessageId = msg.Id.RawValue;
            await db.SaveChangesAsync();
            await Context.ReplyAndDeleteAsync(null, false,
                new LocalEmbedBuilder().Create("Suggestion sent!", Color.Green));
            await ApplyEmotesAsync(msg, cfg);
        }

        [Name("Approve Suggestion")]
        [Command("approve", "ar")]
        [Description("Approves a suggestion by its Id with a optional reason")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ApproveSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id.RawValue);
            if (suggestion?.MessageId == null)
            {
                await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                return;
            }

            var msg = (IUserMessage) await Context.Guild.GetTextChannel(cfg.Channel.Value)
                .GetMessageAsync(suggestion.MessageId.Value);
            if (msg == null) return;
            var sugstMessage = await CommentSuggestion(Context.Member, msg, reason, Color.Green);
            await RespondUser(suggestion, sugstMessage, reason);
        }

        [Name("Decline Suggestion")]
        [Command("decline", "dr")]
        [Description("Decline a suggestion by its ID with a optional reason")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task DeclineSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id.RawValue);
            if (suggestion?.MessageId == null)
            {
                await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                return;
            }

            if (!(await Context.Guild.GetTextChannel(cfg.Channel.Value)
                .GetMessageAsync(suggestion.MessageId.Value) is IUserMessage msg)) return;
            var sugstMessage = await CommentSuggestion(Context.Member, msg, reason, Color.Red);
            await RespondUser(suggestion, sugstMessage, reason);
        }

        [Name("Comment Suggestion")]
        [Command("Comment", "rr")]
        [Description("Adds a comment onto a suggestion, usable by user suggesting and server admin")]
        public async Task CommentSuggestionAsync(int id, [Remainder] string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (!cfg.Channel.HasValue) return;
            var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id.RawValue);
            if (!Context.Member.Permissions.Has(Permission.ManageGuild) &&
                Context.User.Id.RawValue != suggestion.UserId) return;

            if (suggestion?.MessageId == null)
            {
                await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red);
                return;
            }

            if (!(await Context.Guild.GetTextChannel(cfg.Channel.Value)
                .GetMessageAsync(suggestion.MessageId.Value) is IUserMessage msg)) return;
            var sugstMessage = await CommentSuggestion(Context.Member, msg, reason);
            if (Context.User.Id.RawValue != suggestion.UserId) await RespondUser(suggestion, sugstMessage, reason);
        }

        private async Task ApplyEmotesAsync(IUserMessage msg, SuggestionConfig cfg)
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
            for (var i = 0; i < result.Count; i++)
            {
                var x = result[i];
                await msg.AddReactionAsync(x);
            }
        }

        private async Task<string> CommentSuggestion(CachedMember user, IUserMessage msg, string message,
            Color? color = null)
        {
            var embed = msg.Embeds.First().ToEmbedBuilder();
            if (color.HasValue) embed.Color = color;
            embed.AddField(user.DisplayName, message);
            await msg.ModifyAsync(x => x.Embed = embed.Build());
            return embed.Description;
        }

        private async Task RespondUser(Database.Tables.Moderation.Suggestion suggestion, string sugst, string response)
        {
            try
            {
                var suggestUser = Context.Guild.GetMember(suggestion.UserId);
                if (suggestUser == null) return;
                var embed = new LocalEmbedBuilder().Create(
                    $"Your suggestion got a response in {Context.Guild.Name}!\n" +
                    "Suggestion:\n" +
                    $"{sugst.Truncate(300)}\n" +
                    $"Answer from {Context.User}:\n" +
                    $"{response.Truncate(1200)}", Context.Colour.Get(Context.Guild.Id.RawValue));
                if (suggestUser.DmChannel != null) await suggestUser.DmChannel.SendMessageAsync(null, false, embed.Build());
                else
                {
                    var dm = await suggestUser.CreateDmChannelAsync();
                    await dm.SendMessageAsync(null, false, embed.Build());
                }
            }
            catch
            {
                /*IGNORE*/
            }
        }
    }
}