using Discord.WebSocket;

namespace Hanekawa.Entities.Log
{
    public class UserUnbanned
    {
        public SocketUser User { get; set; }
        public SocketGuild Guild { get; set; }
    }
}
