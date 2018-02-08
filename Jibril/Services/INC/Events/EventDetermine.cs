using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.HungerGames.Data;

namespace Jibril.Services.INC.Calculate
{
    public class EventDetermine
    {
        public static void EventHandler()
        {
            var rand = new Random();
            var chance = rand.Next(0, 1000);
            
        }
    }
}
