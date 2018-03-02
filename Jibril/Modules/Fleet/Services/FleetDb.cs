using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Fleet.Services
{
    public class FleetDb
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.Password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.Server;
        private readonly string username = DbInfo.Username;

        public FleetDb(string table)
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

        public static List<string> CheckFleetName(string name)
        {
            var result = new List<string>();
            var database = new FleetDb("hanekawa");
            var str = $"SELECT * FROM fleet WHERE name = '{name}'";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var fleetName = (string) tableName["name"];
                result.Add(fleetName);
            }

            database.CloseConnection();
            return result;
        }

        public static List<string> CheckFleetMemberStatus(IUser user)
        {
            var result = new List<string>();
            var database = new FleetDb("hanekawa");
            var str = $"SELECT * FROM exp WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var fleetName = (string) tableName["fleetName"];
                result.Add(fleetName);
            }

            database.CloseConnection();
            return result;
        }

        public static void UpdateFleetProfile(IUser user, string name)
        {
            var database = new FleetDb("hanekawa");
            var str = $"UPDATE exp SET fleetName = '{name}' WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddFleet(string name)
        {
            var database = new FleetDb("hanekawa");
            var str = $"INSERT INTO fleet (name, members) VALUES ('{name}', '1')";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddFleetMemberCount(string name)
        {
            var database = new FleetDb("hanekawa");
            var str = $"UPDATE fleet SET members = members + '1' WHERE name = '{name}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveFleetMemberCount(string name)
        {
            var database = new FleetDb("hanekawa");
            var str = $"UPDATE fleet SET members = members - '1' WHERE name = '{name}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<FleetList> FleetDetails(string name)
        {
            var result = new List<FleetList>();
            var database = new FleetDb("hanekawa");
            var str = "";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var user_id = (string) tableName["user_id"];
                var rank = (string) tableName["rank"];
                var joined = (DateTime) tableName["joined"];
                var exp = (int) tableName["exp"];
                var level = (int) tableName["level"];
                var totalexp = (int) tableName["totalexp"];

                result.Add(new FleetList
                {
                    Exp = exp,
                    Level = level,
                    Totalexp = totalexp
                });
            }

            database.CloseConnection();
            return result;
        }
    }

    public class FleetNormDb
    {
        private readonly string database = DbInfo.DbFleet;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.Password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.Server;
        private readonly string username = DbInfo.Username;

        public FleetNormDb(string table)
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

        public static void CreateFleet(IUser user, string name)
        {
            var database = new FleetNormDb("oshino");
            var str = $"CREATE TABLE `oshino`.`{name}` (" +
                      $"`id` INT NOT NULL AUTO_INCREMENT," +
                      $"`user_id` VARCHAR(99) NULL," +
                      $"`rank` VARCHAR(99) NULL," +
                      $"`joined` DATETIME NULL," +
                      $"`exp` INT(11) NULL DEFAULT '0'," +
                      $"`level` INT(11) NULL DEFAULT '1'," +
                      $"`totalexp` INT(11) NULL DEFAULT '0'," +
                      $"PRIMARY KEY(`id`)); ";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddLeader(IUser user, string name)
        {
            var database = new FleetNormDb("oshino");
            var str2 =
                $"INSERT INTO `oshino`.`{name}` (user_id, rank, joined ) VALUES ('{user.Id}', 'leader', curtime())";
            var tableName2 = database.FireCommand(str2);
            database.CloseConnection();
        }

        public static List<string> GetLeader(IUser user, string name)
        {
            try
            {
                var result = new List<string>();
                var database = new FleetNormDb("oshino");
                var str = $"SELECT * FROM `oshino`.`{name}` WHERE id = '1'";
                var tableName = database.FireCommand(str);

                while (tableName.Read())
                {
                    var userid = (string) tableName["user_id"];
                    result.Add(userid);
                }

                database.CloseConnection();
                return result;
            }
            catch
            {
                var result = new List<string>();
                return result;
            }
        }

        public static List<string> RankCheck(IUser user, string name)
        {
            var result = new List<string>();
            var database = new FleetNormDb("oshino");
            //var str = $"SELECT * FROM exp WHERE user_id = {user.Id}";
            var str = $"SELECT * FROM `oshino`.`{name}` WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var rank = (string) tableName["rank"];
                result.Add(rank);
            }

            return result;
        }

        public static void AddMember(IUser user, string name)
        {
            var database = new FleetNormDb("oshino");
            var str =
                $"INSERT INTO `oshino`.`{name}` (user_id, rank, joined ) VALUES ('{user.Id}', 'member', curtime())";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveMember(IUser user, string name)
        {
            var database = new FleetNormDb("oshino");
            var str = $"DELETE FROM `oshino`.`{name}` WHERE user_id = '{user.Id}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }
    }

    public class FleetList
    {
        public int Exp { get; set; }
        public int Level { get; set; }
        public int Totalexp { get; set; }
    }
}