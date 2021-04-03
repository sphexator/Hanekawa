using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Disqord;
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

        public static async Task PaginatedReply(this HanekawaCommandContext ctx, List<string> content,
            IUser userIcon,
            string authorTitle,
            string title = null,
            int pageSize = 5)
            => await PaginationBuilder(ctx, content, userIcon.GetAvatarUrl(), authorTitle ?? userIcon.Name, title, pageSize);

        public static async Task PaginatedReply(this HanekawaCommandContext ctx, List<string> content, 
            IGuild guildIcon,
            string authorTitle,
            string title = null,
            int pageSize = 5) 
            => await PaginationBuilder(ctx, content, guildIcon.GetIconUrl(), authorTitle ?? guildIcon.Name, title, pageSize);

        private static async Task PaginationBuilder(HanekawaCommandContext ctx, IReadOnlyList<string> content, 
            string icon,
            string authorTitle, 
            string title, 
            int pageSize)
        {
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var color = ctx.Colour.Get(ctx.Guild.Id.RawValue);
            for (var i = 0; i < content.Count;)
            {
                for (var j = 0; j < pageSize; j++)
                {
                    if (i >= content.Count) continue;
                    var x = content[i];
                    sb.AppendLine(x);
                    i++;
                }

                var page = pages.Count + 1;
                var maxPage = Convert.ToInt32(Math.Ceiling((double) content.Count / pageSize));
                pages.Add(new Page(new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder {Name = authorTitle, IconUrl = icon},
                    Title = title,
                    Description = sb.ToString(),
                    Color = color,
                    Footer = new LocalEmbedFooterBuilder {Text = $"Page: {page}/{maxPage}"}
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

        public static bool IsOverriden(this MethodInfo methodInfo)
        {
            return methodInfo == methodInfo?.GetBaseDefinition();
        }

        public static string Inspect(this object obj)
        {
            var type = obj.GetType();
            var overridenToString = type.GetMethod("ToString").IsOverriden();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();

            var sb = new StringBuilder("```css\n");

            //doesnt work
            sb.AppendLine($"{{{type}: '{(overridenToString ? obj.ToString() : "")}'}}");
            sb.AppendLine();

            var maxLength = props.Max(x => x.Name.Length);

            foreach (var prop in props)
            {
                sb.Append($"#{prop.Name.PadRight(maxLength, ' ')} - ");

                object value = null;

                try
                {
                    value = prop.GetValue(obj);
                }
                catch (TargetInvocationException) { } //c# bad

                static string DateTimeNicefy(DateTimeOffset dateTime)
                {
                    return $"{dateTime.Day}/{dateTime.Month}/{dateTime.Year} {dateTime.TimeOfDay.ToString("g").Split('.').First()}";
                }

                static string NicefyNamespace(Type inType)
                {
                    var str = inType.ToString();

                    return str.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
                }

                type = value?.GetType();
                if (type is null)
                {
                    continue;
                }

                overridenToString = type.GetMethods()
                    .First(method => method.GetParameters().Length == 0 && method.Name == "ToString")
                    .IsOverriden();

                switch (value)
                {
                    case IEnumerable collection when !(value is string):
                        var count = collection.Cast<object>().Count();
                        sb.AppendLine($"[{count} item{(count == 1 ? "" : "s")}]");
                        break;

                    // case Enum @enum:
                    //     sb.AppendLine($"[{@enum.Humanize()}]");
                    //     break;

                    case string str:
                        sb.AppendLine($"[\"{str}\"]");
                        break;

                    case Task task:
                        var returnT = type.GetGenericArguments();
                        sb.AppendLine(returnT.Length > 0
                            ? $"[Task<{string.Join(", ", returnT.Select(NicefyNamespace))}>]"
                            : "[Task]");
                        break;

                    case DateTime dt:
                        sb.AppendLine($"[{DateTimeNicefy(dt)}]");
                        break;

                    case DateTimeOffset dto:
                        sb.AppendLine($"[{DateTimeNicefy(dto)}]");
                        break;

                    default:
                        if (overridenToString)
                        {
                            sb.AppendLine($"[{value}]");
                        }
                        else
                        {
                            if (type?.IsValueType == false)
                            {
                                var niceName = NicefyNamespace(type);
                                sb.AppendLine($"[{niceName}]");
                            }
                            else
                            {
                                sb.AppendLine($"[{value}]");
                            }
                        }
                        break;
                }
            }

            sb.AppendLine("```");

            return sb.ToString();
        }
    }
}