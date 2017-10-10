using Discord;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class GameDatabase
    {
        public static string DB = @"Data Source = data\database.db;Version=3;Foreign Keys=ON;";
        public static void UpdateClass(IUser user, string shipClass)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET shipclass = '{shipClass}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }
    }
}