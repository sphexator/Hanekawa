using System.Collections.Generic;
using Jibril.Data.Variables;
using Jibril.Services.Reaction.List;
using MySql.Data.MySqlClient;

namespace Jibril.Services.Reaction
{
    public class ReactionDb
    {
        private readonly string _database = "DbInfo.DbNorm";
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = "DbInfo.Password";
        private readonly string _server = "DbInfo.Server";
        private readonly string _username = "DbInfo.Username";

        public ReactionDb(string table)
        {
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = _server,
                UserID = _username,
                Password = _password,
                Database = _database,
                SslMode = MySqlSslMode.None,
                Pooling = false
            };
            var connectionString = stringBuilder.ToString();
            _dbConnection = new MySqlConnection(connectionString);
            _dbConnection.Open();
        }

        public MySqlDataReader FireCommand(string query)
        {
            if (_dbConnection == null)
                return null;
            var command = new MySqlCommand(query, _dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (_dbConnection != null)
                _dbConnection.Close();
        }

        public static void InsertReactionMessage(string msgid, string channelid, int counter)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"INSERT INTO reaction (msgid, chid, counter) VALUES ('{msgid}', '{channelid}', '{counter}')";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<ReactionList> ReactionData(string msgid)
        {
            var result = new List<ReactionList>();
            var database = new ReactionDb("hanekawa");
            var str = $"SELECT * FROM reaction WHERE msgid = {msgid}";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var messageid = (string) reader["msgid"];
                var chid = (string) reader["chid"];
                var counter = (int) reader["counter"];
                var sent = (string) reader["sent"];

                result.Add(new ReactionList
                {
                    Msgid = msgid,
                    Chid = chid,
                    Counter = counter,
                    Sent = sent
                });
            }
            database.CloseConnection();
            return result;
        }

        public static void AddReaction(string msgid)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"UPDATE reaction SET counter = counter + '1' WHERE msgid = '{msgid}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveReaction(string msgid)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"UPDATE reaction SET counter = counter - '1' WHERE msgid = '{msgid}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void ReactionMsgPosted(string msgid)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"UPDATE reaction SET sent = 'yes' WHERE msgid = '{msgid}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }
    }
}