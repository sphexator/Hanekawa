using Discord.WebSocket;

namespace Hanekawa.Entities.Log
{
    public class UserJoined
    {
        public SocketGuildUser User { get; set; }
    }
}
