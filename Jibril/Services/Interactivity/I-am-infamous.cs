using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Jibril.Services.Interactivity
{
    public class I_am_infamous
    {
        private readonly DiscordSocketClient _client;

        public I_am_infamous(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += MessageCounter;
        }

        private Task MessageCounter(SocketMessage arg)
        {
            throw new NotImplementedException();
        }
    }
}
