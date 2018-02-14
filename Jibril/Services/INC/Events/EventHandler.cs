using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Events.Types;

namespace Jibril.Services.INC.Events
{
    public class EventHandler
    {
        public static string EventManager(Profile profile)
        {
            var evt = ChanceGenerator.EventDeterminator(profile);
            if (evt == ChanceGenerator.LootName)
            {
                var response = Types.Loot.LootEvent();
                return response;
            }
            else if (evt == ChanceGenerator.KillName)
            {
                var response = Kill.KillEvent(profile); 
                return response;

            }
            else if (evt == ChanceGenerator.IdleName)
            {
                var response = Idle.IdleEvent();
                return response;
            }
            else if (evt == ChanceGenerator.MeetName)
            {
                var response = Meet.MeetEvent();
                return response;
            }
            else if (evt == ChanceGenerator.HackName)
            {
                var response = Hack.HackEvent(profile);
                return response;
            }
            else if (evt == ChanceGenerator.DieName)
            {
                var response = Die.DieEvent();
                return response;
            }
            else if (evt == ChanceGenerator.SleepName)
            {
                //TODO:Sleep
            }
            else if (evt == ChanceGenerator.EatName)
            {
                //var response = Sleep;
                //TODO: Eat
            }
            else
            {
                var msg = Idle.IdleEvent();
                return msg;
            }

            return null;
        }
    }
}