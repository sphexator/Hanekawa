using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class UserBanned
    {
        public SocketUser User { get; set; }
        public SocketGuild Guild { get; set; }
    }
}