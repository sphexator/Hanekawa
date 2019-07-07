using System.Threading.Tasks;
using Discord.WebSocket;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public class HanekawaContext : CommandContext
    {
        public HanekawaContext(DiscordSocketClient client, SocketUserMessage msg, SocketGuildUser user, ColourService colour)
        {
            Client = client;
            Message = msg;
            User = user;
            _colour = colour;
            Guild = user.Guild;
            Channel = msg.Channel as SocketTextChannel;
        }

        public SocketUserMessage Message { get; }
        public DiscordSocketClient Client { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }
        private ColourService _colour { get; }

        public async Task ReplyAsyncTest(string content)
        {
            // TODO: Implement replyasync directly into command context with guild specific guild colour
        }

        public async Task ErrorAsync(string content)
        {
            // TODO: Implement various ways to do error messages, maybe ok?
        }
    }
}