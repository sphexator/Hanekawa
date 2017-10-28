using Discord;
using Jibril.Modules.Gambling.Services;
using Jibril.Modules.Game.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Welcome.Services
{
    public class DatabaseInsert
    {
        public static void InserToDb(IUser user)
        {
            var userdata = DatabaseService.CheckUser(user);
            if (userdata.Count <= 0)
            {
                DatabaseService.EnterUser(user);
                GameDatabase.CreateGameDBEntry(user);
                GambleDB.CreateInventory(user);
                return;
            }
            else return;
        }
    }
}
