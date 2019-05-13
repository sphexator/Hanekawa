using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class UserLeft
    {
        public SocketGuildUser User { get; set; }
    }
}