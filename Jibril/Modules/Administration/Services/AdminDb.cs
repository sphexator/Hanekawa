using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.List;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Administration.Services
{
    public class AdminDb
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        public AdminDb(string table)
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

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
                return null;
            var command = new MySqlCommand(query, dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }

        public static List<string> CheckExistingUserWarn(IUser user)
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM warnings WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userId = (string) reader["user_id"];
                result.Add(userId);
            }

            database.CloseConnection();
            return result;
        }

        public static void AddWarn(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"UPDATE warnings SET warnings = warnings + '1', total_warnings = total_warnings + '1' WHERE user_id = '{user.Id}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void CreateWarn(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str = $"INSERT INTO warnings (user_id, warnings, total_warnings) VALUES ('{user.Id}', '1', '1')";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<WarnAmount> GetWarnings(IUser user)
        {
            var result = new List<WarnAmount>();

            var database = new AdminDb("hanekawa");

            var str = $"SELECT * FROM warnings WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var warnings = (int) reader["warnings"];
                var total_warnings = (int) reader["total_warnings"];

                result.Add(new WarnAmount
                {
                    Warnings = warnings,
                    Total_warnings = total_warnings
                });
            }

            database.CloseConnection();
            return result;
        }


        // Cases

        public static void AddActionCase(IUser user, DateTime now)
        {
            var database = new AdminDb("hanekawa");
            var str = $"INSERT INTO modlog (user_id, date) VALUES ('{user.Id}', '{now}')";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<int> GetActionCaseID(DateTime time)
        {
            var result = new List<int>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM modlog WHERE date = '{time}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var suggestNr = (int) reader["id"];
                result.Add(suggestNr);
            }

            database.CloseConnection();
            return result;
        }

        public static void UpdateActionCase(string msgid, int id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE modlog SET msgid = '{msgid}' WHERE id = '{id}'";
            var tablename = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RespondActionCase(uint casenr, string response, IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE modlog SET responduser = '{user.Id}', response = '{response}' WHERE id = '{casenr}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<string> ActionCaseMessage(uint casenr)
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM modlog WHERE id = '{casenr}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var msgid = (string) reader["msgid"];

                result.Add(msgid);
            }

            database.CloseConnection();
            return result;
        }

        public static List<ActionCase> ActionCaseList(uint casenr)
        {
            var result = new List<ActionCase>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM modlog WHERE id = '{casenr}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userid = (string) reader["user_id"];
                var msgid = (string) reader["msgid"];

                result.Add(new ActionCase
                {
                    User_id = userid,
                    Msgid = msgid
                });
            }

            database.CloseConnection();
            return result;
        }

        // Adding bans + timestamp
        public static List<ulong> CheckBanList(IUser user)
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM banlog WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var userid = (ulong) reader["user_id"];
                result.Add(userid);
            }

            database.CloseConnection();
            return result;
        }

        /// <summary> Adds user to banlog, unbans in 7 days. </summary>
        public static void AddBan(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"INSERT INTO banlog (user_id, date, unbanDate, counter) VALUES ('{user.Id}', '{DateTime.Today}', '{DateTime.Today.AddDays(7)}', '1')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Adds user to banlog, unbans in 2 months. </summary>
        public static void AddBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"INSERT INTO banlog (user_id, date, unbanDate, counter) VALUES ('{user.Id}', '{DateTime.Today}', '{DateTime.Today.AddMonths(2)}', '1')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 7 days. </summary>
        public static void UpdateBan(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"UPDATE banlog SET date = '{DateTime.Now.Date}', unbanDate = {DateTime.Now.Date.AddDays(7)}, counter = counter + '1' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 2 months. </summary>
        public static void UpdateBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"UPDATE banlog SET date = '{DateTime.Now.Date.AddMonths(2)}', counter = counter + '1' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 2 months. </summary>
        public static void UpdateBanPerm(ulong user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"UPDATE banlog SET date = '{DateTime.Now.Date.AddMonths(2)}', counter = counter + '1' WHERE user_id = {user}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Admin manual update banlog, sets user to be unbanned in 2 months. </summary>
        public static void AdminBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE banlog SET date = '{DateTime.Now.Date.AddMonths(2)}' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Admin manual update banlog, sets user to be unbanned in 2 months. </summary>
        public static void AdminBanPerm(ulong user)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE banlog SET date = '{DateTime.Now.Date.AddMonths(2)}' WHERE user_id = {user}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Gets users set to be unbanned today </summary>
        public static List<ulong> GetBannedUsers()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM banlog WHERE date = '{DateTime.Now.Date}'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var user = (ulong) reader["user_id"];
                result.Add(user);
            }

            database.CloseConnection();
            return result;
        }

        // ----------------- Perspective API ----------------- //

        public static void AddToxicityValue(double tvalue, double newAvg, IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str =
                $"UPDATE exp SET toxicityvalue = '{tvalue}', toxicitymsgcount = toxicitymsgcount + 1, toxicityavg = '{newAvg}' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        // ----------------- Get text from db----------------- //

        public static List<string> GetRules(string guildid)
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM guildinfo WHERE guild = '{guildid}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var rules = (string) reader["rules"];

                result.Add(rules);
            }

            database.CloseConnection();
            return result;
        }

        public static List<string> Getfaq(string guildid)
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM guildinfo WHERE guild = '{guildid}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var rules = (string) reader["faq"];

                result.Add(rules);
            }

            database.CloseConnection();
            return result;
        }
    }

    public class WarningDB
    {
        private readonly string database = DbInfo.DbWarn;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        public WarningDB(string table)
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

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
                return null;
            var command = new MySqlCommand(query, dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }


        public static void EnterUser(IUser user)
        {
            var database = new WarningDB("senjougahara");
            var str = $"CREATE TABLE `senjougahara`.`{user.Id}` (" +
                      $"`id` INT(11) NOT NULL AUTO_INCREMENT," +
                      $"`staff_id` VARCHAR(45) NULL DEFAULT NULL," +
                      $"`message` LONGTEXT NULL DEFAULT NULL," +
                      $"`warndate` DATETIME NULL DEFAULT '0001-01-01 00:00:00'," +
                      $"PRIMARY KEY(`id`));";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddWarn(IUser user, IUser staff, string msg)
        {
            var database = new WarningDB("senjougahara");
            var addWarnNumb =
                $"INSERT INTO `senjougahara`.`{user.Id}` (staff_id, message, warndate ) VALUES ('{staff.Id}', '{msg}', curtime())";
            var tableName = database.FireCommand(addWarnNumb);
            database.CloseConnection();
        }

        public static List<WarnList> WarnList(IUser user)
        {
            try
            {
                var result = new List<WarnList>();
                var database = new WarningDB("senjougahara");
                var str = $"SELECT * FROM `senjougahara`.`{user.Id}` ORDER BY id DESC LIMIT 5";
                var tableName = database.FireCommand(str);

                while (tableName.Read())
                {
                    var staff_id = (string) tableName["staff_id"];
                    var message = (string) tableName["message"];
                    var date = (DateTime) tableName["warndate"];

                    result.Add(new WarnList
                    {
                        Staff_id = staff_id,
                        Message = message,
                        Date = date
                    });
                }

                database.CloseConnection();
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}