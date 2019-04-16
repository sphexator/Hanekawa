using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;

namespace Hanekawa.Entities
{
    public class HanekawaContext : SocketCommandContext
    {
        public HanekawaContext(DiscordSocketClient client, SocketUserMessage msg, DbService db) : base(client, msg)
        {
            Client = client;
            Message = msg;
            Channel = msg.Channel as SocketTextChannel;
            User = msg.Author as SocketGuildUser;
            Guild = (msg.Channel as SocketTextChannel)?.Guild;
            Db = db;
        }

        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }
        public SocketGuildUser User { get; }
        public SocketUserMessage Message { get; }
        public DbService Db { get; }
    }
}
