using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class UserJoined
    {
        public SocketGuildUser User { get; set; }
    }
}
