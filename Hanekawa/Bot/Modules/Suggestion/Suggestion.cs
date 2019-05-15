﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Qmmands;

namespace Hanekawa.Bot.Modules.Suggestion
{
    [Name("Suggestion")]
    [Description("Module for creating suggestions for a server, adds up/down votes for users to show if they think it's a good idea or not.")]
    public class Suggestion : InteractiveBase
    {
        [Name("Suggest")]
        [Command("suggest")]
        [Description("Sends a suggestion to the server, if they have it enabled")]
        [Remarks("suggest this is a suggestion")]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var caseId = await db.CreateSuggestion(Context.User, Context.Guild, DateTime.UtcNow);
                var embed = new EmbedBuilder().CreateDefault(suggestion, Context.Guild.Id);
                embed.Author = new EmbedAuthorBuilder { IconUrl = Context.User.GetAvatar(), Name = Context.User.GetName() };
                embed.Footer = new EmbedFooterBuilder { Text = $"Suggestion ID: {caseId.Id}" };
                embed.Timestamp = DateTimeOffset.UtcNow;
                if (Context.Message.Attachments.Count > 0) embed.WithImageUrl(Context.Message.Attachments.First().Url);
                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).ReplyAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Suggestion sent!", Color.Green.RawValue).Build());
                await ApplyEmotesAsync(msg, cfg);
            }
        }

        [Name("Approve Suggestion")]
        [Command("approve")]
        [Description("Approves a suggestion by its Id with a optional reason")]
        [Remarks("approve 69 this is a good suggestion")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ApproveSuggestionAsync(int id, [Remainder]string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion?.MessageId == null)
                {
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red.RawValue);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason, Color.Green);
                await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        [Name("Decline Suggestion")]
        [Command("decline")]
        [Description("Decline a suggestion by its ID with a optional reason")]
        [Remarks("decline 69 not now")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DeclineSuggestionAsync(int id, [Remainder]string reason = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (!cfg.Channel.HasValue) return;
                var suggestion = await db.Suggestions.FindAsync(id, Context.Guild.Id);
                if (suggestion?.MessageId == null)
                {
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red.RawValue);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason, Color.Red);
                await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        [Name("Comment Suggestion")]
        [Command("Comment")]
        [Description("Adds a comment onto a suggestion, usable by user suggesting and server admin")]
        [Remarks("comment 69 go on")]
        public async Task CommentSuggestionAsync(int id, [Remainder]string reason = null)
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
                    await Context.ReplyAsync("Couldn't find a suggestion with that id.", Color.Red.RawValue);
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.Channel.Value).GetMessageAsync(suggestion.MessageId.Value) as IUserMessage;
                if (msg == null) return;
                var sugstMessage = await CommentSuggestion(Context.User, msg, reason);
                if(Context.User.Id != suggestion.UserId) await RespondUser(suggestion, sugstMessage, reason);
            }
        }

        [Name("Suggestion Channel")]
        [Command("suggest channel")]
        [Description("Sets a channel as channel to receive suggestions. don't mention a channel to disable suggestions.")]
        [Remarks("suggest channel #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestionChannelAsync(SocketTextChannel channel = null)
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
                if (channel == null) channel = Context.Channel;
                cfg.Channel = channel.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"All suggestions will now be sent to {channel.Mention} !",
                    Color.Green.RawValue);
            }
        }

        [Name("Suggest Yes Emote")]
        [Command("suggestion set yes")]
        [Description("Set custom yes emote for suggestions")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestEmoteYesAsync(Emote emote = null)
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

                cfg.EmoteYes = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Suggest No Emote")]
        [Command("suggest set no")]
        [Description("Set custom no emote for suggestions")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestEmoteNoAsync(Emote emote = null)
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

                cfg.EmoteNo = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
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

            var result = new List<IEmote> { iYes, iNo };
            for (var i = 0; i < result.Count; i++)
            {
                var x = result[i];
                await msg.AddReactionAsync(x);
            }
        }

        private async Task<string> CommentSuggestion(SocketGuildUser user, IUserMessage msg, string message, Color? color = null)
        {
            var embed = msg.Embeds.First().ToEmbedBuilder();
            if(color.HasValue) embed.Color = color;
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
                    new EmbedBuilder().CreateDefault(
                        $"Your suggestion got a response in {Context.Guild.Name}!\n" +
                        "Suggestion:\n" +
                        $"{sugst.Truncate(300)}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{response.Truncate(1200)}", Context.Guild.Id));
            }
            catch
            {
                /*IGNORE*/
            }
        }
    }
}