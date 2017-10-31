using Discord;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.List;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Administration.Services
{
    public class AdminDb
    {
        private string _table { get; set; }
        string server = DbInfo.server;
        string database = DbInfo.DbNorm;
        string username = DbInfo.username;
        string password = DbInfo.password;
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public AdminDb(string table)
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

        public static List<String> CheckExistingUserWarn(IUser user)
        {
            var result = new List<String>();
            var database = new AdminDb("hanekawa");
            var str = ($"SELECT * FROM warnings WHERE user_id = '{user.Id}'");
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
            var str = ($"UPDATE warnings SET warnings = warnings + '1', total_warnings = total_warnings + '1' WHERE user_id = '{user.Id}'");
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static void CreateWarn(IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str = ($"INSERT INTO warnings (user_id, warnings, total_warnings) VALUES ('{user.Id}', '1', '1')");
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static List<WarnAmount> GetWarnings(IUser user)
        {
            var result = new List<WarnAmount>();

            var database = new AdminDb("hanekawa");

            var str = ($"SELECT * FROM warnings WHERE user_id = {user.Id}");
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
            return;
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
            return;
        }

        public static void RespondActionCase(uint casenr, string response, IUser user)
        {
            var database = new AdminDb("hanekawa");
            var str = $"UPDATE modlog SET responduser = '{user.Id}', response = '{response}' WHERE id = '{casenr}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static List<String> ActionCaseMessage(uint casenr)
        {
            var result = new List<String>();
            var database = new AdminDb("hanekawa");
            var str = ($"SELECT * FROM modlog WHERE id = '{casenr}'");
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var msgid = (string)reader["msgid"];

                result.Add(msgid);
            }
            database.CloseConnection();
            return result;
        }
    }

    public class WarningDB
    {
        private string _table { get; set; }
        string server = DbInfo.server;
        string database = DbInfo.DbWarn;
        string username = DbInfo.username;
        string password = DbInfo.password;
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public WarningDB(string table)
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


        public static void EnterUser(IUser user)
        {
            var database = new WarningDB("senjougahara");
            var str = ($"CREATE TABLE `senjougahara`.`{user.Id}` (" +
                $"`id` INT(11) NOT NULL AUTO_INCREMENT," +
                $"`staff_id` VARCHAR(45) NULL DEFAULT NULL," +
                $"`message` LONGTEXT NULL DEFAULT NULL," +
                $"`warndate` DATETIME NULL DEFAULT '0001-01-01 00:00:00'," +
                $"PRIMARY KEY(`id`));");
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;

        }

        public static void AddWarn(IUser user, IUser staff, string msg)
        {
            var database = new WarningDB("senjougahara");
            var addWarnNumb = ($"INSERT INTO `senjougahara`.`{user.Id}` (staff_id, message, warndate ) VALUES ('{staff.Id}', '{msg}', curtime())");
            var tableName = database.FireCommand(addWarnNumb);
            database.CloseConnection();
            return;
        }

        public static List<WarnList> WarnList(IUser user)
        {
            var result = new List<WarnList>();
            var database = new WarningDB("senjougahara");
            var str = ($"SELECT * FROM `senjougahara`.`{user.Id}` ORDER BY id DESC LIMIT 5");
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var staff_id = (string)tableName["staff_id"];
                var message = (string)tableName["message"];
                var date = (DateTime)tableName["warndate"];

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
    }
}
