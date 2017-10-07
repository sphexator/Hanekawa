using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using Discord;
using Jibril.Services.Level.Lists;

namespace Jibril.Services.Level.Services
{
    public class DbService
    {
        public static string DB = @"Data Source = data\database.db;Version=3;Foreign Keys=ON;";
        public static void AddExperience(IUser user, int exp, int credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET xp = xp + '{exp}', total_xp = total_xp + '{exp}', tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void Levelup(IUser user, int exp)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET level = level + 1, xp = '{exp}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }
    }
}
