using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace Jibril.Services.HungerGames
{
    public class HungerGames
    {
        private readonly DiscordSocketClient _client;
        private readonly List<ulong> _eventStartMsg;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
        }


    }
}
