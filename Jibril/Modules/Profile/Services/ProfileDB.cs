using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileDB
    {
        public static string DB = @"Data Source = data/database.sqlite;Version=3;Foreign Keys=ON;";
        public static string CheckProfileBG(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
            }
            return "o";
        }
    }
}
