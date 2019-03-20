﻿using Discord;
using Discord.WebSocket;

namespace Hanekawa.Entities.LogEntities
{
    public class MessageUpdated
    {
        public Cacheable<IMessage, ulong> OldMessage { get; set; }
        public SocketMessage NewMessage { get; set; }
        public ISocketMessageChannel Channel { get; set; }
    }
}