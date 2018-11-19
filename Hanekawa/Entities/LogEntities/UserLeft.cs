using Discord.WebSocket;

namespace Hanekawa.Entities.Log
{
    public class UserLeft
    {
        public SocketGuildUser User { get; set; }
    }
}
