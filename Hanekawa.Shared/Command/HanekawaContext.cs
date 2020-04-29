using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public class HanekawaContext : CommandContext
    {
        public virtual DiscordClient Bot { get; }

        public virtual string Prefix { get; }

        public virtual CachedUserMessage Message { get; }

        public virtual CachedTextChannel Channel => Message.Channel as CachedTextChannel;

        public virtual CachedUser User => Message.Author;

        public virtual CachedMember Member => User as CachedMember;

        public virtual CachedGuild Guild => Member?.Guild;
        
        public virtual ColourService Colour { get; }

        public HanekawaContext(DiscordClient bot, CachedUserMessage message, string prefix, ColourService colour, IServiceProvider provider) : base(provider)
        {
            Bot = bot;
            Prefix = prefix;
            Message = message;
            Colour = colour;
        }

        public async Task<IUserMessage> ReplyAsync(string content) =>
    await Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
    {
        Color = Colour.Get(Guild.Id.RawValue),
        Description = content
    }.Build());

        public async Task<IUserMessage> ReplyAsync(string content, Color color) =>
            await Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
            {
                Color = color,
                Description = content
            }.Build());

        public async Task<IUserMessage> ReplyAsync(LocalEmbedBuilder embed)
        {
            if (embed.Color == null || embed.Color == Color.Purple) embed.Color = Colour.Get(Guild.Id.RawValue);
            return await Channel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task PaginatedReply(List<string> content, CachedMember userIcon, string authorTitle,
            string title = null)
        {
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var color = Colour.Get(Guild.Id.RawValue);
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
                    Author = new LocalEmbedAuthorBuilder { Name = authorTitle, IconUrl = userIcon.GetAvatarUrl() },
                    Title = title,
                    Description = sb.ToString(),
                    Color = color
                }.Build()));
                sb.Clear();
            }

            await Bot.GetInteractivity()
                .StartMenuAsync(Channel, new PagedMenu(User.Id.RawValue, new DefaultPageProvider(pages)));
        }

        public async Task PaginatedReply(List<string> content, CachedGuild guildIcon, string authorTitle,
            string title = null)
        {
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var color = Colour.Get(Guild.Id.RawValue);
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
                    Author = new LocalEmbedAuthorBuilder { Name = authorTitle, IconUrl = guildIcon.GetIconUrl() },
                    Title = title,
                    Description = sb.ToString(),
                    Color = color
                }.Build()));
                sb.Clear();
            }

            await Bot.GetInteractivity()
                .StartMenuAsync(Channel, new PagedMenu(User.Id.RawValue, new DefaultPageProvider(pages)));
        }

        public async Task<RestUserMessage> ReplyAndDeleteAsync(string content, bool isTts = false,
            LocalEmbedBuilder embed = null,
            TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(25);
            var message = await Channel.SendMessageAsync(content, isTts, embed.Build(), LocalMentions.NoEveryone)
                .ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }
    }
}