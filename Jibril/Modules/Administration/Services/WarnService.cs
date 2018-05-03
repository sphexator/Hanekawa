using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Jibril.Modules.Administration.Services
{
    public class WarnService
    {
        private readonly DiscordSocketClient _client;
        public WarnService(DiscordSocketClient client)
        {
            _client = client;
        }
    }
}
