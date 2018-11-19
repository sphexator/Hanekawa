using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class UserUnbanned
    {
        public SocketUser User { get; set; }
        public SocketGuild Guild { get; set; }
    }
}
