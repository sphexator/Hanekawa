using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services
{
    public class DatabaseService
    {
        public static string DB = @"Data Source = data\database.db;Version=3;Foreign Keys=ON;";
        public static List<String> CheckUser(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<String>();
                var sql = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
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

        public static void EnterUser(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                var sql = $"INSERT INTO exp (user_id, username, tokens, level, xp ) VALUES ('{user.Id}', 'username', '0', '1', '1')";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }
        
        public static List<UserData> UserData(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<UserData>();
                string sql = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var userId = (string)reader["user_id"];
                    var userName = (string)reader["username"];
                    var currentTokens = (uint)reader["tokens"];
                    var event_tokens = (uint)reader["event_tokens"];
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
