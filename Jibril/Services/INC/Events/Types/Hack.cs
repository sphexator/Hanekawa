using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace Jibril.Services.INC.Events.Types
{
    public class Hack
    {
        public static string HackEvent(SocketGuildUser usr)
        {
            //TODO: Add Database change for obtaining everything
            const string response = "Hacked the system, obtaining every single item";
            return response;
        }
    }
}
