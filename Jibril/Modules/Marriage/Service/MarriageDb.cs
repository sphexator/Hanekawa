using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Marriage.Service
{
    public class MarriageDb
    {
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.password;
        private const bool Pooling = false;
        private readonly string _server = DbInfo.server;
        private readonly string _username = DbInfo.username;

        private MarriageDb()
        {
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
            var database = new MarriageDb();
            var str = $"SELECT * FROM waifu WHERE user = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (string)exec["user_id"];

                result.Add(userId);
            }
            database.CloseConnection();
            return result;
        }

        public static IEnumerable<MarriageDbVariables> MarriageData(ulong usr)
        {
            var result = new List<MarriageDbVariables>();
            var database = new MarriageDb();
            var str = $"SELECT * FROM waifu WHERE user = '{usr}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var user = (ulong)exec["user"];
                var name = (string)exec["name"];
                var claim = (ulong)exec["claim"];
                var claimname = (string)exec["claimname"];
                var timer = (DateTime)exec["timer"];
                var rank = (int)exec["rank"];
                result.Add(new MarriageDbVariables
                {
                    Userid = user,
                    Name = name,
                    Claim = claim,
                    ClaimName = claimname,
                    Timer = timer,
                    Rank = rank
                });
            }
            database.CloseConnection();
            return result;
        }

        public static IEnumerable<MarriageDbVariables> GetMarriageData()
        {
            var result = new List<MarriageDbVariables>();
            var database = new MarriageDb();
            const string str = "SELECT * FROM waifu";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var user = (ulong)exec["user"];
                var name = (string)exec["name"];
                var claim = (ulong)exec["claim"];
                var claimname = (string)exec["claimname"];
                var timer = (DateTime)exec["timer"];
                var rank = (int) exec["rank"];
                result.Add(new MarriageDbVariables
                {
                    Userid = user,
                    Name = name,
                    Claim = claim,
                    ClaimName = claimname,
                    Timer = timer,
                    Rank = rank
                });
            }
            database.CloseConnection();
            return result;
        }
    }
    public class MarriageDbVariables
    {
        public ulong Userid { get; set; }
        public string Name { get; set; }
        public ulong Claim { get; set; }
        public string ClaimName { get; set; }
        public DateTime Timer { get; set; }
        public int Rank { get; set; }
    }
}
