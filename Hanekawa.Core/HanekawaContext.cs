using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace Hanekawa.Core
{
    public class HanekawaContext : CommandContext
    {
        public HanekawaContext(DiscordSocketClient client, SocketUserMessage msg, SocketGuildUser user)
        {
            Client = client;
            Message = msg;
            User = user;
            Guild = user.Guild;
            Channel = msg.Channel as SocketTextChannel;
        }

        public SocketUserMessage Message { get; }
        public DiscordSocketClient Client { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }

        public async Task ReplyAsyncTest(string content)
        {

        }

        public async Task ErrorAsync(string content)
        {

        }
    }
}