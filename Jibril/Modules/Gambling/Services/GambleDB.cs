using Discord;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Modules.Gambling.Services
{
    public class GambleDB
    {
        public static string DB = @"Data Source = data\database.db;Version=3;Foreign Keys=ON;";
        public static void AddCredit(IUser user, uint credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void RemoveCredit(IUser user, int credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET tokens = tokens - '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }
    }
}
