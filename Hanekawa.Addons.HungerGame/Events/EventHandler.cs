using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Calculate;
using Hanekawa.Addons.HungerGame.Events.Types;

namespace Hanekawa.Addons.HungerGame.Events
{
    internal class EventHandler
    {
        public static string EventManager(HungerGameLive profile, DbService db)
        {
            var evt = ChanceGenerator.EventDeterminator(profile);
            switch (evt)
            {
                case ChanceGenerator.LootName:
                {
                    var response = Loot.LootEvent(profile, db);
                    return response;
                }
                case ChanceGenerator.KillName:
                {
                    var response = Kill.KillEvent(profile, db);
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
                    var response = Hack.HackEvent(profile, db);
                    return response;
                }
                case ChanceGenerator.DieName:
                {
                    var response = Die.DieEvent(profile, db);
                    return response;
                }
                case ChanceGenerator.SleepName:
                {
                    var response = Sleep.SleepEvent(profile, db);
                    return response;
                }
                case ChanceGenerator.EatName:
                {
                    var response = Eat.EatEvent(profile, db);
                    return response;
                }
            }

            var msg = Idle.IdleEvent();
            return msg;
        }
    }
}