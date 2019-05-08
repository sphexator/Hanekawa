using Discord;
using Discord.WebSocket;
using Qmmands;

namespace Hanekawa.Core
{
    public class HanekawaContext : CommandContext
    {
        public HanekawaContext(DiscordSocketClient client, IUserMessage msg, SocketGuildUser user)
        {
            Client = client;
            Message = msg;
            User = user;
            Guild = user.Guild;
            Channel = msg.Channel as SocketTextChannel;
        }

        public IUserMessage Message { get; }

        public DiscordSocketClient Client { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }
    }
}