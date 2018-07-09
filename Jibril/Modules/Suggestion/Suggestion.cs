using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;

namespace Jibril.Modules.Suggestion
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
                var caseId = await db.CreateSuggestion(Context.User, DateTime.UtcNow);
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
                    new EmbedBuilder().Reply("Suggestion sent!", Color.Red.RawValue).Build());
            }
        }

        [Command("approve", RunMode = RunMode.Async)]
        [Alias("ar")]
        [RequireContext(ContextType.Guild)]
        public async Task ApproveAsync(uint id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id);
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

        [Command("deny", RunMode = RunMode.Async)]
        [Alias("dr")]
        [RequireContext(ContextType.Guild)]
        public async Task DenyAsync(uint id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id);
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
        public async Task CommentAsync(uint id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                var suggestion = await db.Suggestions.FindAsync(id);
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
    }
}