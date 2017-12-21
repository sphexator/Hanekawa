﻿using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using Jibril.Services.Level.Lists;
using MySql.Data.MySqlClient;

namespace Jibril.Services
{
    public class DatabaseService
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        private DatabaseService(string table)
        {
            _table = table;
            var stringBuilder = new MySqlConnectionStringBuilder
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

        private string _table { get; }

        private MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
                return null;
            var command = new MySqlCommand(query, dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        private void CloseConnection()
        {
            if (dbConnection != null)
                dbConnection.Close();
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
            var exec = database.FireCommand(str);
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
                var event_tokens = (uint) exec["event_tokens"];
                var level = (int) exec["level"];
                var exp = (int) exec["xp"];
                var totalExp = (int) exec["total_xp"];
                var daily = (DateTime) exec["daily"];
                var cooldown = (DateTime) exec["cooldown"];
                var voice_timer = (DateTime) exec["voice_timer"];
                var joinDate = (DateTime) exec["joindate"];
                var fleetName = (string) exec["fleetName"];
                var shipClass = (string) exec["shipClass"];
                var profilepic = (string) exec["profilepic"];
                var gameCD = (DateTime) exec["game_cooldown"];
                var gambleCD = (DateTime) exec["gambling_cooldown"];
                var hasrole = (string) exec["hasrole"];

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
                    JoinDateTime = joinDate,
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

        public static List<UserData> UserData(ulong Id)
        {
            var result = new List<UserData>();
            var database = new DatabaseService("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = '{Id}'";
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
                var joinDate = (DateTime)exec["joindate"];
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
                    JoinDateTime = joinDate,
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