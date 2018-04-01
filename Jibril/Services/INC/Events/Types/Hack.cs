using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Hack
    {
        public static string HackEvent(Profile profile)
        {
            DatabaseHungerGame.AddEverything(profile.Player.UserId);
            const string response = "Hacked the system, obtaining every single item";
            return response;
        }
    }
}
