using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;

namespace Hanekawa.Shared.Command.Extensions
{
    public static class Extensions
    {
        public static bool HasMentionPrefix(this IMessage message, IUser user, out string prefix, out string parsed)
        {
            var content = message.Content;
            parsed = "";
            prefix = "";
            if (content.Length <= 3 || content[0] != '<' || content[1] != '@')
                return false;

            var endPos = content.IndexOf('>');
            if (endPos == -1) return false;

            if (content.Length < endPos + 2 || content[endPos + 1] != ' ')
                return false;

            if (!TryParseUser(content.Substring(0, endPos + 1), out var userId))
                return false;

            if (userId != user.Id.RawValue) return false;
            parsed = content.Substring(endPos + 2);

            prefix = user.Mention;
            return true;
        }

        public static bool TryParseUser(string text, out ulong userId)
        {
            if (text.Length >= 3 && text[0] == '<' && text[1] == '@' && text[^1] == '>')
            {
                text = text.Length >= 4 && text[2] == '!'
                    ? text[3..^1]
                    : text[2..^1];

                if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out userId))
                    return true;
            }

            userId = 0;
            return false;
        }

        public static async Task<IUserMessage> ReplyAsync(this HanekawaCommandContext ctx, string content) =>
            await ctx.Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
            {
                Color = ctx.Colour.Get(ctx.Guild.Id.RawValue),
                Description = content
            }.Build());

        public static async Task<IUserMessage> ReplyAsync(this HanekawaCommandContext ctx, LocalEmbedBuilder embed) =>
            await ctx.Channel.SendMessageAsync(null, false, embed.Build());

        public static async Task<IUserMessage> ReplyAsync(this HanekawaCommandContext ctx, string content, Color color) =>
            await ctx.Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
            {
                Color = color,
                Description = content
            }.Build());

        public static async Task PaginatedReply(this HanekawaCommandContext ctx, List<string> content, CachedMember userIcon,
            string authorTitle,
            string title = null)
        {
            authorTitle ??= userIcon.DisplayName;
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var color = ctx.Colour.Get(ctx.Guild.Id.RawValue);
            for (var i = 0; i < content.Count;)
            {
                for (var j = 0; j < 5; j++)
                {
                    if (i >= content.Count) continue;
                    var x = content[i];
                    sb.AppendLine(x);
                    i++;
                }

                pages.Add(new Page(new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder {Name = authorTitle, IconUrl = userIcon.GetAvatarUrl()},
                    Title = title,
                    Description = sb.ToString(),
                    Color = color
                }.Build()));
                sb.Clear();
            }

            await ctx.Bot.GetInteractivity()
                .StartMenuAsync(ctx.Channel, new PagedMenu(ctx.User.Id.RawValue, new DefaultPageProvider(pages)));
        }

        public static async Task PaginatedReply(this HanekawaCommandContext ctx, List<string> content, CachedGuild guildIcon,
            string authorTitle,
            string title = null)
        {
            authorTitle ??= guildIcon.Name;
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var color = ctx.Colour.Get(ctx.Guild.Id.RawValue);
            for (var i = 0; i < content.Count;)
            {
                for (var j = 0; j < 5; j++)
                {
                    if (i >= content.Count) continue;
                    var x = content[i];
                    sb.AppendLine(x);
                    i++;
                }

                pages.Add(new Page(new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder {Name = authorTitle, IconUrl = guildIcon.GetIconUrl()},
                    Title = title,
                    Description = sb.ToString(),
                    Color = color
                }.Build()));
                sb.Clear();
            }

            await ctx.Bot.GetInteractivity()
                .StartMenuAsync(ctx.Channel, new PagedMenu(ctx.User.Id.RawValue, new DefaultPageProvider(pages)));
        }

        public static async Task<RestUserMessage> ReplyAndDeleteAsync(this HanekawaCommandContext ctx, string content,
            bool isTts = false,
            LocalEmbedBuilder embed = null,
            TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(25);
            var message = embed != null
                ? await ctx.Channel.SendMessageAsync(content, isTts, embed.Build(), LocalMentions.NoEveryone)
                    .ConfigureAwait(false)
                : await ctx.Channel.SendMessageAsync(content, isTts, null, LocalMentions.NoEveryone)
                    .ConfigureAwait(false);
            try
            {
                _ = Task.Delay(timeout.Value)
                    .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore
            }
            return message;
        }
    }
}