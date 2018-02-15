using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Sleep
    {
        public static string SleepEvent(Profile profile)
        {
            DatabaseHungerGame.Sleep(profile.Player.UserId);
            return "Fell asleep";
        }
    }
}
