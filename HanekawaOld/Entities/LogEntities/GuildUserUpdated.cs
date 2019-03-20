using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class GuildUserUpdated
    {
        public SocketGuildUser OldUser { get; set; }
        public SocketGuildUser NewUser { get; set; }
    }
}