using Discord;
using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class MessageDeleted
    {
        public Cacheable<IMessage, ulong> OptMsg { get; set; }
        public ISocketMessageChannel Channel { get; set; }
    }
}
