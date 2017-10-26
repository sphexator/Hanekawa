using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Jibril.Services.Level.Lists;
using MySql.Data.MySqlClient;

namespace Jibril.Services.Level.Services
{
    public class LevelDatabase
    {
        private string _table { get; set; }
        string server = "192.168.10.143";
        string database = "hanekawa";
        string username = "admin";
        string password = "jevel123";
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public LevelDatabase(string table)
        {
            try
            {
                _table = table;
                MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = server,
                    UserID = username,
                    Password = password,
                    Database = database,
                    SslMode = MySqlSslMode.None,
                    Pooling = POOLING
                };
                var connectionString = stringBuilder.ToString();
                dbConnection = new MySqlConnection(connectionString);
                dbConnection.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                return null;
            }
            MySqlCommand command = new MySqlCommand(query, dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }
        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        public static void AddExperience(IUser user, int exp, int credit)
        {
            try
            {
                var database = new LevelDatabase("hanekawa");
                var str = $"UPDATE exp SET xp = xp + '{exp}', total_xp = total_xp + '{exp}', tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
                var reader = database.FireCommand(str);
                database.CloseConnection();
                return;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Levelup(IUser user, int exp)
        {
            var database = new LevelDatabase("hanekawa");
            var str = $"UPDATE exp SET level = level + 1, xp = '{exp}' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static void ChangeCooldown(IUser user)
        {
            var database = new LevelDatabase("hanekawa");
            var str = $"UPDATE exp SET cooldown = curtime() WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static void StartVoiceCounter(IUser user)
        {
            var database = new LevelDatabase("hanekawa");
            var str = $"UPDATE exp SET voice_timer = curtime() WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static List<UserData> GetLeaderBoard()
        {
            var database = new LevelDatabase("hanekawa");
            var result = new List<UserData>();
            var str = "SELECT * FROM exp ORDER BY total_xp DESC LIMIT 10";
            var reader = database.FireCommand(str);

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
            database.CloseConnection();
            return result;
        }
    }
}
