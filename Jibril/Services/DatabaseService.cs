using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using Jibril.Services.Level.Lists;
using MySql.Data.MySqlClient;

namespace Jibril.Services
{
    public class DatabaseService
    {
        private const bool Pooling = false;
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.Password;
        private readonly string _server = DbInfo.Server;
        private readonly string _username = DbInfo.Username;

        private DatabaseService(string table)
        {
            _table = table;
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = _server,
                UserID = _username,
                Password = _password,
                Database = _database,
                SslMode = MySqlSslMode.None,
                Pooling = Pooling
            };
            var connectionString = stringBuilder.ToString();
            _dbConnection = new MySqlConnection(connectionString);
            _dbConnection.Open();
        }

        private string _table { get; }

        private MySqlDataReader FireCommand(string query)
        {
            if (_dbConnection == null)
                return null;
            var command = new MySqlCommand(query, _dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        private void CloseConnection()
        {
            _dbConnection?.Close();
        }

        public static List<string> CheckUser(IUser user)
        {
            var result = new List<string>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string) exec["user_id"];

                result.Add(userId);
            }

            return result;
        }

        public static void EnterUser(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str =
                $"INSERT INTO exp (user_id, username, tokens, level, xp, joindate) VALUES ('{user.Id}', 'username', '0', '1', '1', curtime())";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void UserJoinedDate(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str =
                $"UPDATE exp SET joindate = curtime() WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        //MVP Counter
        public static void AddMessageCounter(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str =
                $"UPDATE exp SET mvpCounter = mvpCounter + 1 WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void ResetMessageCounter()
        {
            var database = new DatabaseService("hanekawa");
            var str = $"UPDATE exp SET mvpCounter = '0', mvpimmunity = '0'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void SetNewMvp(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str = $"UPDATE exp SET mvpimmunity = '1' WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void SetMvPIgnore(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str = $"UPDATE exp SET mvpignore = '1' WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveMvPIgnore(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str = $"UPDATE exp SET mvpignore = '0' WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<ulong> GetActiveUsers()
        {
            var database = new DatabaseService("hanekawa");
            var result = new List<ulong>();
            const string str = "SELECT * FROM exp WHERE mvpimmunity = '0' && mvpignore = '0' ORDER BY mvpCounter DESC LIMIT 5";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userId = (string) reader["user_id"];
                var userIdConvert = ulong.Parse(userId);

                result.Add(userIdConvert);
            }

            database.CloseConnection();
            return result;
        }

        public static List<UserData> UserData(IUser user)
        {
            var result = new List<UserData>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string) exec["user_id"];
                var userName = (string) exec["username"];
                var currentTokens = (uint) exec["tokens"];
                var eventTokens = (uint) exec["event_tokens"];
                var level = (int) exec["level"];
                var exp = (int) exec["xp"];
                var totalExp = (int) exec["total_xp"];
                var daily = (DateTime) exec["daily"];
                var cooldown = (DateTime) exec["cooldown"];
                var voiceTimer = (DateTime) exec["voice_timer"];
                var joinDate = (DateTime) exec["joindate"];
                var fleetName = (string) exec["fleetName"];
                var shipClass = (string) exec["shipClass"];
                var profilepic = (string) exec["profilepic"];
                var gameCd = (DateTime) exec["game_cooldown"];
                var gambleCd = (DateTime) exec["gambling_cooldown"];
                var hasrole = (string) exec["hasrole"];
                var toxicityvalue = (double) exec["toxicityvalue"];
                var toxicitymsgcount = (int) exec["toxicitymsgcount"];
                var toxicityavg = (double) exec["toxicityavg"];
                var mvpCounter = (int) exec["mvpCounter"];
                var mvpImmunity = (int) exec["mvpimmunity"];
                var mvpIgnore = (int) exec["mvpignore"];
                var rep = (int) exec["rep"];
                var repcd = (DateTime) exec["repcd"];
                var firstMsg = (DateTime) exec["firstmsg"];
                var lastMsg = (DateTime) exec["lastmsg"];

                result.Add(new UserData
                {
                    UserId = userId,
                    Username = userName,
                    Tokens = currentTokens,
                    Event_tokens = eventTokens,
                    Level = level,
                    Xp = exp,
                    Total_xp = totalExp,
                    Daily = daily,
                    Cooldown = cooldown,
                    Voice_timer = voiceTimer,
                    JoinDateTime = joinDate,
                    FleetName = fleetName,
                    ShipClass = shipClass,
                    Profilepic = profilepic,
                    GameCD = gameCd,
                    BetCD = gambleCd,
                    Hasrole = hasrole,
                    Toxicityvalue = toxicityvalue,
                    Toxicitymsgcount = toxicitymsgcount,
                    Toxicityavg = toxicityavg,
                    MvPCounter = mvpCounter,
                    MvPImmunity = mvpImmunity,
                    MvPIgnore = mvpIgnore,
                    Rep = rep,
                    Repcd = repcd,
                    FirstMsg = firstMsg,
                    LastMsg = lastMsg
                });
            }

            database.CloseConnection();
            return result;
        }

        public static List<UserData> UserData(ulong id)
        {
            var result = new List<UserData>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string) exec["user_id"];
                var userName = (string) exec["username"];
                var currentTokens = (uint) exec["tokens"];
                var eventTokens = (uint) exec["event_tokens"];
                var level = (int) exec["level"];
                var exp = (int) exec["xp"];
                var totalExp = (int) exec["total_xp"];
                var daily = (DateTime) exec["daily"];
                var cooldown = (DateTime) exec["cooldown"];
                var voiceTimer = (DateTime) exec["voice_timer"];
                var joinDate = (DateTime) exec["joindate"];
                var fleetName = (string) exec["fleetName"];
                var shipClass = (string) exec["shipClass"];
                var profilepic = (string) exec["profilepic"];
                var gameCd = (DateTime) exec["game_cooldown"];
                var gambleCd = (DateTime) exec["gambling_cooldown"];
                var hasrole = (string) exec["hasrole"];
                var toxicityvalue = (double) exec["toxicityvalue"];
                var toxicitymsgcount = (int) exec["toxicitymsgcount"];
                var toxicityavg = (double) exec["toxicityavg"];
                var mvpCounter = (int)exec["mvpCounter"];
                var mvpImmunity = (int)exec["mvpimmunity"];
                var mvpIgnore = (int)exec["mvpignore"];
                var rep = (int) exec["rep"];
                var repcd = (DateTime) exec["repcd"];
                var firstMsg = (DateTime) exec["firstmsg"];
                var lastMsg = (DateTime) exec["lastmsg"];

                result.Add(new UserData
                {
                    UserId = userId,
                    Username = userName,
                    Tokens = currentTokens,
                    Event_tokens = eventTokens,
                    Level = level,
                    Xp = exp,
                    Total_xp = totalExp,
                    Daily = daily,
                    Cooldown = cooldown,
                    Voice_timer = voiceTimer,
                    JoinDateTime = joinDate,
                    FleetName = fleetName,
                    ShipClass = shipClass,
                    Profilepic = profilepic,
                    GameCD = gameCd,
                    BetCD = gambleCd,
                    Hasrole = hasrole,
                    Toxicityvalue = toxicityvalue,
                    Toxicitymsgcount = toxicitymsgcount,
                    Toxicityavg = toxicityavg,
                    MvPCounter = mvpCounter,
                    MvPImmunity = mvpImmunity,
                    MvPIgnore = mvpIgnore,
                    Rep = rep,
                    Repcd = repcd,
                    FirstMsg = firstMsg,
                    LastMsg = lastMsg
                });
            }

            database.CloseConnection();
            return result;
        }

        public static void AddFirstMessage(IUser user)
        {
            var database = new DatabaseService("hanekawa");
            var str = $"UPDATE exp SET firstmsg = curtime() WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
    }
}