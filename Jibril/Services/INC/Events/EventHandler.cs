using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Calculate;

namespace Jibril.Services.INC.Events
{
    public class EventHandler
    {
        public static void EventManager(Player player, Weapons weapons, Consumables consumables)
        {
            var evt = ChanceGenerator.EventDeterminator(player, weapons, consumables);
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
