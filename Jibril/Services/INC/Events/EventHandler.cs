using Hanekawa.Services.Entities.Tables;
using Hanekawa.Services.INC.Calculate;
using Hanekawa.Services.INC.Events.Types;

namespace Hanekawa.Services.INC.Events
{
    public static class EventHandler
    {
        public static string EventManager(HungerGameLive profile)
        {
            var evt = ChanceGenerator.EventDeterminator(profile);
            switch (evt)
            {
                case ChanceGenerator.LootName:
                {
                    var response = Types.Loot.LootEvent(profile);
                    return response;
                }
                case ChanceGenerator.KillName:
                {
                    var response = Kill.KillEvent(profile);
                    return response;
                }
                case ChanceGenerator.IdleName:
                {
                    var response = Idle.IdleEvent();
                    return response;
                }
                case ChanceGenerator.MeetName:
                {
                    var response = Meet.MeetEvent();
                    return response;
                }
                case ChanceGenerator.HackName:
                {
                    var response = Hack.HackEvent(profile);
                    return response;
                }
                case ChanceGenerator.DieName:
                {
                    var response = Die.DieEvent(profile);
                    return response;
                }
                case ChanceGenerator.SleepName:
                {
                    var response = Sleep.SleepEvent(profile);
                    return response;
                }
                case ChanceGenerator.EatName:
                {
                    var response = Eat.EatEvent(profile);
                    return response;
                }
            }

            var msg = Idle.IdleEvent();
            return msg;
        }
    }
}