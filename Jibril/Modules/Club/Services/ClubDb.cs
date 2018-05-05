using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Jibril.Data.Variables;
using Jibril.Extensions;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Club.Services
{
    public class ClubDb
    {
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.Password;
        private const bool Pooling = false;
        private readonly string _server = DbInfo.Server;
        private readonly string _username = DbInfo.Username;

        private ClubDb(string table)
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

        public static void AddClubMember(IUser user, int id, string clubName)
        {
            AddMember(user, id, clubName);
            IncreaseMemberCount(id);
        }
        private static void AddMember(IUser user, int id, string clubName)
        {
            var database = new ClubDb("hanekawa");
            var str = $"INSERT into fleet (userid, clubid, name, clubname, rank, joindate) VALUES ('{user.Id}', '{id}', '{user.Username.RemoveSpecialCharacters()}', '{clubName}', 3, curtime())";
            database.FireCommand(str);
            database.CloseConnection();
        }
        private static void IncreaseMemberCount(int id)
        {
            var database = new ClubDb("hanekawa");
            var str = $"UPDATE fleetinfo SET members = members + 1 WHERE id = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void RemoveClubMember(IUser user, int id)
        {
            RemoveMember(user, id);
            DecreaseMemberCount(id);
        }
        private static void RemoveMember(IUser user, int id)
        {
            var database = new ClubDb("hanekawa");
            var str = $"DELETE FROM fleet WHERE userid = '{user.Id}' && clubid = '{id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        private static void DecreaseMemberCount(int id)
        {
            var database = new ClubDb("hanekawa");
            var str2 = $"UPDATE fleetinfo SET members = members - 1 WHERE id = {id}";
            database.FireCommand(str2);
            database.CloseConnection();
        }
        public static void Promote(IUser user)
        {
            var database = new ClubDb("hanekawa");
            var str = $"UPDATE fleet SET rank = rank - 1 WHERE userid = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void Demote(IUser user)
        {
            var database = new ClubDb("hanekawa");
            var str = $"UPDATE fleet SET rank = rank + 1 WHERE userid = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void PromoteLeader(IUser user, int id)
        {
            SetLeaderFleet(user, id);
            SetLeaderFleetInfo(user, id);
        }
        private static void SetLeaderFleet(IUser user, int id)
        {
            var database = new ClubDb("hanekawa");
            var str1 = $"UPDATE fleet SET rank = 1 WHERE userid = '{user.Id}' && clubid = '{id}'";
            database.FireCommand(str1);
            database.CloseConnection();
        }
        private static void SetLeaderFleetInfo(IUser user, int id)
        {
            var database = new ClubDb("hanekawa");
            var str = $"UPDATE fleetinfo SET leader = '{user.Id}' WHERE id = '{id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static IReadOnlyCollection<FleetUserInfo> UserClubData(IUser user)
        {
            var result = new List<FleetUserInfo>();
            var database = new ClubDb("hanekawa");
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
        public static IReadOnlyCollection<FleetUserInfo> ClubData(int id)
        {
            var result = new List<FleetUserInfo>();
            var database = new ClubDb("hanekawa");
            var str = $"SELECT * FROM fleet WHERE clubid = '{id}'";
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
        public static List<FleetInfo> GetClubs()
        {
            var result = new List<FleetInfo>();
            var database = new ClubDb("hanekawa");
            var str = $"SELECT * FROM fleetinfo";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var id = (int)exec["id"];
                var clubName = (string)exec["clubname"];
                var creationdate = (DateTime)exec["creationdate"];
                var members = (int) exec["members"];
                var leader = (ulong) exec["leader"];
                var channelId = (ulong) exec["channelid"];
                var roleId = (ulong) exec["roleid"];

                result.Add(new FleetInfo
                {
                    Id = id,
                    Name = clubName,
                    Members = members,
                    CreationTime = creationdate,
                    Leader = leader,
                    ChannelId = channelId,
                    RoleId = roleId
                });
            }
            database.CloseConnection();
            return result;
        }
        public static void ChannelCreated(int id, ulong role, ulong channel)
        {
            var database = new ClubDb("hanekawa");
            var str = $"UPDATE fleetinfo SET channelid = '{channel}', roleid = '{role}' WHERE id = '{id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        public static void CreateClub(IUser user, string name)
        {
            var fixClubName = name.RemoveSpecialCharacters();
            var fixUsername = user.Username.RemoveSpecialCharacters();
            ClubCreateInsertp1(fixClubName, user.Id);
            var info = GetLeaderId(user.Id);
            ClubCreateInsertp2(user.Id, fixClubName, fixUsername, info);
        }
        public static void DeleteClub(int id)
        {
            RemoveFromfleet(id);
            RemoveFromFleetInfo(id);
        }
        private static void RemoveFromfleet(int id)
        {
            var database = new ClubDb("hanekawa");
            var str = $"DELETE FROM fleet WHERE clubid = '{id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }
        private static void RemoveFromFleetInfo(int id)
        {
            var database = new ClubDb("hanekawa");
            var str = $"DELETE FROM fleetinfo WHERE id = '{id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        private static void ClubCreateInsertp1(string name, ulong id)
        {
            var database = new ClubDb("hanekawa");
            var str1 = $"INSERT into fleetinfo (clubname, creationdate, members, leader) VALUES ('{name}', curtime(), 1, '{id}')";
            database.FireCommand(str1);
            database.CloseConnection();
        }
        private static void ClubCreateInsertp2(ulong userid, string name, string username, IEnumerable<FleetInfo> info)
        {;
            var database = new ClubDb("hanekawa");
            var info1 = info.FirstOrDefault();
            var str = $"INSERT into fleet (userid, clubid, name, clubname, rank, joindate) VALUES ('{userid}', '{info1.Id}', '{username}', '{name}', 1, curtime())";
            database.FireCommand(str);
            database.CloseConnection();
        }
        private static IReadOnlyCollection<FleetInfo> GetLeaderId(ulong baid)
        {
            var result = new List<FleetInfo>();
            var database = new ClubDb("hanekawa");
            var str = $"SELECT * FROM fleetinfo WHERE leader = '{baid}'";
            var exec = database.FireCommand(str);
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
            database.CloseConnection();
            return result;
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
        public ulong ChannelId { get; set; }
        public ulong RoleId { get; set; }
    }
}