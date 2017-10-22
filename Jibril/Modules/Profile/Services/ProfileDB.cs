using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileDB
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";

        public static void AddProfileURL(IUser user, string url)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET profilepic = '{url}' WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void RemoveProfileURL(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET profilepic = 'o' WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }

        }
    }
}
