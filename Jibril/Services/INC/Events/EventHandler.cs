using System;
using System.Collections.Generic;
using System.Linq;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Events.Types;

namespace Jibril.Services.INC.Events
{
    public class EventHandler
    {
        public static void EventManager(Profile profile)
        {
            var evt = ChanceGenerator.EventDeterminator(profile);
            if (evt == ChanceGenerator.LootName)
            {
                Types.Loot.LootEvent();
            }
            if (evt == ChanceGenerator.KillName)
            {
                Kill.KillEvent();
            }
            if (evt == ChanceGenerator.IdleName)
            {
                Idle.IdleEvent();
            }
            if (evt == ChanceGenerator.MeetName)
            {
                Meet.MeetEvent();
            }
            if (evt == ChanceGenerator.HackName)
            {
                Hack.HackEvent();
            }
            if (evt == ChanceGenerator.DieName)
            {
                Die.DieEvent();
            }
            if (evt == ChanceGenerator.SleepName)
            {
                //TODO:Sleep
            }
            if (evt == ChanceGenerator.EatName)
            {
                //TODO: Eat
            }
        }
    }
}