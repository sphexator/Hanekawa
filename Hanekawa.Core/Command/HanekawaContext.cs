using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Shared.Interactive;
using Hanekawa.Shared.Interactive.Paginator;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public class HanekawaContext : CommandContext
    {
        public HanekawaContext(DiscordSocketClient client, SocketUserMessage msg, SocketGuildUser user, ColourService colour, InteractiveService interactive)
        {
            Client = client;
            Message = msg;
            User = user;
            Colour = colour;
            Interactive = interactive;
            Guild = user.Guild;
            Channel = msg.Channel as SocketTextChannel;
        }

        public SocketUserMessage Message { get; }
        public DiscordSocketClient Client { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }
        private ColourService Colour { get; }
        private InteractiveService Interactive { get; }

        public async Task ReplyAsyncTest(string content) =>
            await Channel.SendMessageAsync(null, false, new EmbedBuilder
            {
                Color = Colour.Get(Guild.Id),
                Description = content
            }.Build());

        public async Task ReplyAsyncTest(EmbedBuilder embed) 
            => await Channel.SendMessageAsync(null, false, embed.Build());

        public async Task ErrorAsync(string content)
        {
            // TODO: Implement various ways to do error messages, maybe ok?
        }

        public async Task ReplyPaginated(IReadOnlyList<string> pages, SocketUser userIcon, string authorName, string title = null, int count = 5)
            => await SendPaginator(pages, userIcon.GetAvatarUrl() ?? userIcon.GetDefaultAvatarUrl(), authorName, title, count).ConfigureAwait(false);

        public async Task ReplyPaginated(IReadOnlyList<string> pages, SocketGuild guildIcon, string authorName, string title = null, int count = 5)
            => await SendPaginator(pages, guildIcon.IconUrl, authorName, title, count).ConfigureAwait(false);

        private async Task SendPaginator(IReadOnlyList<string> pages, string icon, string authorName, string title, int count = 5)
        {
            await Interactive.SendPaginatedMessageAsync(this, new PaginatedMessage
            {
                Color = Colour.Get(Guild.Id),
                Author = new EmbedAuthorBuilder { IconUrl = icon, Name = authorName },
                Title = title,
                Pages = PageBuilder(pages, count),
                Options = new PaginatedAppearanceOptions
                {
                    First = new Emoji("⏮"),
                    Back = new Emoji("◀"),
                    Next = new Emoji("▶"),
                    Last = new Emoji("⏭"),
                    Stop = null,
                    Jump = null,
                    Info = null
                }
            });
        }

        private IEnumerable<string> PageBuilder(IReadOnlyList<string> list, int count)
        {
            var pages = new List<string>();
            for (var i = 0; i < list.Count;)
            {
                var page = new StringBuilder();
                for (var j = 0; j < count; j++)
                {
                    if (i >= list.Count) continue;
                    var index = list[i];
                    page.AppendLine(index);
                    i++;
                }

                pages.Add(page.ToString());
            }

            if (pages.Count != 0) return pages;
            pages.Add("No items in list");
            return pages;
        }
    }
}