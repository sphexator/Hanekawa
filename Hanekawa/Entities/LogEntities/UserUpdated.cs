using Discord.WebSocket;

namespace Hanekawa.Entities.Log
{
    public class UserUpdated
    {
        public SocketUser OldUser { get; set; }
        public SocketUser NewUser { get; set; }
    }
}