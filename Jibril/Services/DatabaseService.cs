using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Level.Lists;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services
{
    public class DatabaseService
    {
        private string _table { get; set; }
        string server = "192.168.10.143";
        string database = "hanekawa";
        string username = "admin";
        string password = "jevel123";
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public DatabaseService(string table)
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

        public static List<String> CheckUser(IUser user)
        {
            var result = new List<String>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string)exec["user_id"];

                result.Add(userId);
            }
            return result;
        }

        public static void EnterUser(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str = $"INSERT INTO exp (user_id, username, tokens, level, xp ) VALUES ('{user.Id}', 'username', '0', '1', '1')";
            var exec = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static List<UserData> UserData(IUser user)
        {
            var result = new List<UserData>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string)exec["user_id"];
                var userName = (string)exec["username"];
                var currentTokens = (uint)exec["tokens"];
                var event_tokens = (uint)exec["event_tokens"];
                var level = (int)exec["level"];
                var exp = (int)exec["xp"];
                var totalExp = (int)exec["total_xp"];
                var daily = (DateTime)exec["daily"];
                var cooldown = (DateTime)exec["cooldown"];
                var voice_timer = (DateTime)exec["voice_timer"];
                var fleetName = (string)exec["fleetName"];
                var shipClass = (string)exec["shipClass"];
                var profilepic = (string)exec["profilepic"];
                var gameCD = (DateTime)exec["game_cooldown"];
                var gambleCD = (DateTime)exec["gambling_cooldown"];
                var hasrole = (string)exec["hasrole"];

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
