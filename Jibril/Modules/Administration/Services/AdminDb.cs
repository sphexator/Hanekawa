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
        private readonly string password = DbInfo.Password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.Server;
        private readonly string username = DbInfo.Username;

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
                var userId = (string)reader["user_id"];
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
                var warnings = (int)reader["warnings"];
                var total_warnings = (int)reader["total_warnings"];

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
                var suggestNr = (int)reader["id"];
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
                var msgid = (string)reader["msgid"];

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
                var userid = (string)reader["user_id"];
                var msgid = (string)reader["msgid"];

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
                var userid = (ulong)reader["user_id"];
                result.Add(userid);
            }

            database.CloseConnection();
            return result;
        }

        /// <summary> Adds user to banlog, unbans in 7 days. </summary>
        public static void AddBan(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss");
            var unban = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"INSERT INTO banlog (user_id, date, unbanDate, counter) VALUES ('{user.Id}', '{today}', '{unban}', '1')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Adds user to banlog, unbans in 2 months. </summary>
        public static void AddBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss");
            var unban = DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"INSERT INTO banlog (user_id, date, unbanDate, counter) VALUES ('{user.Id}', '{today}', '{unban}', '1')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 7 days. </summary>
        public static void UpdateBan(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var unban = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"UPDATE banlog SET date = '{DateTime.Now.Date}', unbanDate = {unban}, counter = counter + '1' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 2 months. </summary>
        public static void UpdateBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss");
            var unban = DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"UPDATE banlog SET date = '{unban}', counter = counter + '1' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Updates banlog, sets user to be unbanned in 2 months. </summary>
        public static void UpdateBanPerm(ulong user)
        {
            var database = new AdminDb("hanekawa");
            var unban = DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"UPDATE banlog SET date = '{unban}', counter = counter + '1' WHERE user_id = {user}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Admin manual update banlog, sets user to be unbanned in 2 months. </summary>
        public static void AdminBanPerm(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var unban = DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd H:mm:ss");
            var str = $"UPDATE banlog SET date = '{unban}' WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        /// <summary> Admin manual update banlog, sets user to be unbanned in 2 months. </summary>
        public static void AdminBanPerm(ulong user)
        {
            var database = new AdminDb("hanekawa");
            var unban = DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd H:mm:ss");
            var str = $"UPDATE banlog SET date = '{unban}' WHERE user_id = {user}";
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
                var user = (ulong)reader["user_id"];
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

        public static List<string> GetRules()
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (string)reader["rules"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<string> Getfaq()
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (string)reader["faq"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<string> GetfaqTwo()
        {
            var result = new List<string>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (string)reader["faq2"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }

        // Set Message text
        public static void SetRules(string text)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET rules = '{text}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetFaqOne(string text)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET faq = '{text}' WHERE guild = '339370914724446208";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetFaqTwo(string text)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET faq2 = '{text}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        // Store message IDs
        public static void SetRulesMsgId(ulong id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET rulesmsgid = '{id}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetFaqOneMsgId(ulong id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET faqmsgid = '{id}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetFaqTwoMsgId(ulong id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET faq2msgid = '{id}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetStaffMsgId(ulong id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET staffmsgid = '{id}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void SetLevelInviteMsgId(ulong id)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE guildinfo SET LevelInviteMsgId = '{id}' WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        // Get message IDs
        public static List<ulong> GetRulesMsgId()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (ulong)reader["rulesmsgid"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<ulong> GetFaqOneMsgId()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (ulong)reader["faqmsgid"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<ulong> GetFaqTwoMsgId()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (ulong)reader["faq2msgid"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<ulong> GetStaffMsgId()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (ulong)reader["staffmsgid"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }
        public static List<ulong> GetLevelInviteMsgId()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM guildinfo WHERE guild = '339370914724446208'";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var rules = (ulong)reader["LevelInviteMsgId"];
                result.Add(rules);
            }
            database.CloseConnection();
            return result;
        }

        // Mute stuff
        public static List<MuteRoleConfig> GetMuteRole()
        {
            var result = new List<MuteRoleConfig>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM muteconfig";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var guildid = (ulong)reader["guilid"];
                var muterole = (string)reader["muterole"];

                result.Add(new MuteRoleConfig
                {
                    GuildId = guildid,
                    MuteRole = muterole
                });
            }

            database.CloseConnection();
            return result;
        }

        public static List<MutedUsers> GetMutedUsers()
        {
            var result = new List<MutedUsers>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM mute";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var guildid = (ulong)reader["guildid"];
                var userId = (ulong)reader["user_id"];
                var time = (DateTime)reader["time"];

                result.Add(new MutedUsers
                {
                    Guildid = guildid,
                    Userid = userId,
                    Timer = time
                });
            }

            database.CloseConnection();
            return result;
        }

        public static List<ulong> GetMutedUsersids()
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            const string str = "SELECT * FROM mute";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userId = (ulong)reader["user_id"];
                result.Add(userId);
            }

            database.CloseConnection();
            return result;
        }

        public static List<ulong> GetMutedUsersid(ulong userid)
        {
            var result = new List<ulong>();
            var database = new AdminDb("hanekawa");
            var str = $"SELECT * FROM mute WHERE user_id = {userid}";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userId = (ulong)reader["user_id"];
                result.Add(userId);
            }

            database.CloseConnection();
            return result;
        }

        public static void AddTimedMute(ulong guildid, ulong userId, DateTime timer)
        {
            var database = new AdminDb("hanekawa");
            var unmuteNow = timer.ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"INSERT INTO mute (guildid, user_id, time) VALUES ('{guildid}', '{userId}', '{unmuteNow}')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddTimedMute(ulong guildid, ulong userId)
        {
            var database = new AdminDb("hanekawa");
            var date = DateTime.UtcNow + TimeSpan.FromMinutes(1440);
            var unmuteNow = date.ToString("yyyy-MM-dd H:mm:ss");
            var str =
                $"INSERT INTO mute (guildid, user_id, time) VALUES ('{guildid}', '{userId}', '{unmuteNow}')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveTimedMute(ulong guildid, ulong userId)
        {
            var database = new AdminDb("hanekawa");
            var str = $"DELETE FROM mute WHERE user_id = '{userId}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
    }

    public class WarningDB
    {
        private readonly string database = DbInfo.DbWarn;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.Password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.Server;
        private readonly string username = DbInfo.Username;

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
                      "`id` INT(11) NOT NULL AUTO_INCREMENT," +
                      "`staff_id` VARCHAR(45) NULL DEFAULT NULL," +
                      "`message` LONGTEXT NULL DEFAULT NULL," +
                      "`warndate` DATETIME NULL DEFAULT '0001-01-01 00:00:00'," +
                      "PRIMARY KEY(`id`));";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddWarn(IUser user, IUser staff, string msg)
        {
            var database = new WarningDB("senjougahara");
            var addWarnNumb =
                $"INSERT INTO `senjougahara`.`{user.Id}` (staff_id, message, warndate ) VALUES ('{staff.Id}', '{msg}', curtime())";
            database.FireCommand(addWarnNumb);
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
                    var staffId = (string)tableName["staff_id"];
                    var message = (string)tableName["message"];
                    var date = (DateTime)tableName["warndate"];

                    result.Add(new WarnList
                    {
                        Staff_id = staffId,
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
    public class MuteRoleConfig
    {
        public ulong GuildId { get; set; }
        public string MuteRole { get; set; }
    }

    public class MutedUsers
    {
        public ulong Guildid { get; set; }
        public ulong Userid { get; set; }
        public DateTime Timer { get; set; }
    }
}