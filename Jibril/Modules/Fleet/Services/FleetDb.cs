using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Fleet.Services
{
    public class FleetDb
    {
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.Password;
        private const bool Pooling = false;
        private readonly string _server = DbInfo.Server;
        private readonly string _username = DbInfo.Username;

        private FleetDb(string table)
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

        public static void Promote(IUser user)
        {
            var database = new FleetDb("hanekawa");
            var str = $"UPDATE fleet SET rank = rank - 1";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void Demote(IUser user)
        {
            var database = new FleetDb("hanekawa");
            var str = $"UPDATE fleet SET rank = rank + 1";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void PromoteLeader(IUser user, int id)
        {
            var database = new FleetDb("hanekawa");
            var str1 = $"UPDATE fleet SET rank = 1";
            var str2 = $"UPDATE fleetinfo SET leader = '{user.Id}' WHERE id = '{id}'";
            database.FireCommand(str1);
            database.FireCommand(str2);
            database.CloseConnection();
        }
        //$"UPDATE exp SET joindate = curtime() WHERE user_id = '{user.Id}'";
        public static IReadOnlyCollection<FleetUserInfo> UserFleetData(IUser user)
        {
            var result = new List<FleetUserInfo>();
            var database = new FleetDb("hanekawa");
            var str = $"SELECT * FROM fleet WHERE userid = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (ulong)exec["userid"];
                var clubid = (int)exec["clubid"];
                var name = (string) exec["name"];
                var clubName = (string) exec["clubname"];
                var rank = (int) exec["rank"];
                var joindate = (DateTime) exec["joindate"];

                result.Add(new FleetUserInfo
                {
                    UserId = userId,
                    ClubId = clubid,
                    Name = name,
                    ClubName = clubName,
                    Rank = rank,
                    JoinDateTime = joindate
                });
            }
            database.CloseConnection();
            return result;
        }

        public static IReadOnlyCollection<FleetUserInfo> FleetData(string fleet)
        {
            var result = new List<FleetUserInfo>();
            var database = new FleetDb("hanekawa");
            var str = $"SELECT * FROM fleet WHERE clubName = '{fleet}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var userId = (ulong)exec["userid"];
                var clubid = (int)exec["clubid"];
                var name = (string)exec["name"];
                var clubName = (string)exec["clubname"];
                var rank = (int)exec["rank"];
                var joindate = (DateTime)exec["joindate"];

                result.Add(new FleetUserInfo
                {
                    UserId = userId,
                    ClubId = clubid,
                    Name = name,
                    ClubName = clubName,
                    Rank = rank,
                    JoinDateTime = joindate
                });
            }
            database.CloseConnection();
            return result;
        }

        public static IReadOnlyCollection<FleetInfo> GetFleets()
        {
            var result = new List<FleetInfo>();
            var database = new FleetDb("hanekawa");
            var str = $"SELECT * FROM fleetinfo";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var id = (int)exec["id"];
                var clubName = (string)exec["clubname"];
                var creationdate = (DateTime)exec["creationdate"];
                var members = (int) exec["members"];
                var leader = (ulong) exec["leader"];

                result.Add(new FleetInfo
                {
                    Id = id,
                    Name = clubName,
                    Members = members,
                    CreationTime = creationdate,
                    Leader = leader
                });
            }
            database.CloseConnection();
            return result;
        }

        public static void CreateFleet(IUser user, string name)
        {
            var result = new List<FleetInfo>();
            var database = new FleetDb("hanekawa");
            var str1 = $"INSERT into fleetinfo (clubname, creationdate, members, leader) VALUES ('{name}', 'curtime()', 1, '{user.Id}')";
            var str2 = $"SELECT * FROM fleetinfo WHERE leader = '{user.Id}'";
            database.FireCommand(str1);
            var exec = database.FireCommand(str2);
            while (exec.Read())
            {
                var id = (int)exec["id"];
                var clubName = (string)exec["clubname"];
                var creationdate = (DateTime)exec["creationdate"];
                var members = (int)exec["members"];
                var leader = (ulong)exec["leader"];

                result.Add(new FleetInfo
                {
                    Id = id,
                    Name = clubName,
                    Members = members,
                    CreationTime = creationdate,
                    Leader = leader
                });
            }

            var info = result.FirstOrDefault();
            var str = $"INSERT into fleet (userid, clubid, name, clubname, rank, joindate) VALUES ('{user.Id}', '{info.Id}', '{user.Username}', '{name}', 1, curtime())";
            database.FireCommand(str);
            database.CloseConnection();
        }
    }

    public class FleetUserInfo
    {
        public ulong UserId { get; set; }
        public int ClubId { get; set; }
        public string Name { get; set; }
        public string ClubName { get; set; }
        public int Rank { get; set; }
        public DateTime JoinDateTime { get; set; }
    }

    public class FleetInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public DateTime CreationTime { get; set; }
        public ulong Leader { get; set; }
    }
}