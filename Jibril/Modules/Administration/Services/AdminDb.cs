using Discord;
using Jibril.Modules.Administration.List;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Modules.Administration.Services
{
    public class AdminDb
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";
        public static string WarnDb = @"Data Source = Data/warn.sqlite;Version=3;Foreign Keys=ON;";

        public static List<String> CheckExistingUserWarn(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<String>();
                var sql = $"SELECT * FROM warnings WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var userId = (string)reader["user_id"];
                    result.Add(userId);
                }
                connection.Close();
                return result;
            }
        }

        //Checking Regular DB
        public static void RAddWarn(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE warnings SET warnings = warnings + '1', total_warnings = total_warnings + '1' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static void CreateWarn(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO warnings (user_id, warnings, total_warnings) VALUES ('{user.Id}', '1', '1')";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<WarnAmount> GetWarnings(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<WarnAmount>();
                var sql = ($"SELECT * FROM warnings WHERE user_id = {user.Id}");
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var warnings = (int)reader["warnings"];
                    var total_warnings = (int)reader["total_warnings"];

                    result.Add(new WarnAmount
                    {
                        Warnings = warnings,
                        Total_warnings = total_warnings
                    });
                }
                connection.Close();
                return result;
            }
        }

        // Checking Warning DB

        public static void EnterUser(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(WarnDb))
            {
                connection.Open();
                var sql = ($"CREATE TABLE '{user.Id}' ( " +
                    "`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                    "`staff_id` varchar ( 45 ) DEFAULT NULL, " +
                    "`message` longtext, " +
                    "`warndate` datetime DEFAULT '0001-01-01 00:00:00' )");
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static void AddWarn(IUser user, IUser staff, string msg)
        {
            using (SQLiteConnection connection = new SQLiteConnection(WarnDb))
            {
                connection.Open();
                var sql = ($"INSERT INTO {user.Id} (staff_id, message, warndate ) VALUES ('{staff.Id}', '{msg}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<WarnList> WarnList(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(WarnDb))
            {
                connection.Open();
                var result = new List<WarnList>();
                var sql = ($"SELECT * FROM `{user.Id}` ORDER BY id DESC LIMIT 5");
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var staff_id = (string)reader["staff_id"];
                    var message = (string)reader["message"];
                    var date = (DateTime)reader["warndate"];

                    result.Add(new WarnList
                    {
                        Staff_id = staff_id,
                        Message = message,
                        Date = date
                    });
                }
                connection.Close();
                return result;
            }
        }
    }
}
