using System;
using System.Collections.Generic;
using System.Linq;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;

namespace Jibril.Services.INC.Events
{
    public class EventHandler
    {
        public static void EventManager(Profile profile)
        {
            var evt = ChanceGenerator.EventDeterminator(profile);
            if (evt == ChanceGenerator.LootName)
            {

            }
            if (evt == ChanceGenerator.KillName)
            {

            }
            if (evt == ChanceGenerator.IdleName)
            {

            }
            if (evt == ChanceGenerator.MeetName)
            {

            }
            if (evt == ChanceGenerator.HackName)
            {

            }
            if (evt == ChanceGenerator.DieName)
            {

            }
            if (evt == ChanceGenerator.SleepName)
            {

            }
            if (evt == ChanceGenerator.EatName)
            {

            }
        }
    }
}