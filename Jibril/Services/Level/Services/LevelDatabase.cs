using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using Discord;
using Jibril.Services.Level.Lists;

namespace Jibril.Services.Level.Services
{
    public class LevelDatabase
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";
        public static void AddExperience(IUser user, int exp, int credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET xp = xp + '{exp}', total_xp = total_xp + '{exp}', tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return;
        }

        public static void Levelup(IUser user, int exp)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET level = level + 1, xp = '{exp}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return;
        }

        public static void ChangeCooldown(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET cooldown = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static void StartVoiceCounter(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET voice_timer = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<UserData> GetLeaderBoard()
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<UserData>();
                var sql = "SELECT * FROM exp ORDER BY total_xp DESC LIMIT 10";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    var userId = (string)reader["user_id"];
                    var userName = (string)reader["username"];
                    var currentTokens = (int)reader["tokens"];
                    var event_tokens = (int)reader["event_tokens"];
                    var level = (int)reader["level"];
                    var exp = (int)reader["xp"];
                    var totalExp = (int)reader["total_xp"];
                    var daily = (DateTime)reader["daily"];
                    var cooldown = (DateTime)reader["cooldown"];
                    var voice_timer = (DateTime)reader["voice_timer"];
                    var fleetName = (string)reader["fleetName"];
                    var shipClass = (string)reader["shipClass"];
                    var profilepic = (string)reader["shipclass"];
                    var gameCD = (DateTime)reader["game_cooldown"];
                    var gambleCD = (DateTime)reader["gambling_cooldown"];
                    var hasrole = (string)reader["hasrole"];

                    result.Add(new UserData
                    {
                        UserId = userId,
                        Username = userName,
                        Tokens = currentTokens,
                        Event_tokens = event_tokens,
                        Level = level,
                        Xp = exp,
                        Total_xp = totalExp,
                        Daily = daily,
                        Cooldown = cooldown,
                        Voice_timer = voice_timer,
                        FleetName = fleetName,
                        ShipClass = shipClass,
                        Profilepic = profilepic,
                        GameCD = gameCD,
                        BetCD = gambleCD,
                        Hasrole = hasrole
                    });
                }
                connection.Close();
                return result;
            }
        }
    }
}
