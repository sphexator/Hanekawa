using System.Collections.Generic;
using Jibril.Data.Variables;
using Jibril.Services.Reaction.List;
using MySql.Data.MySqlClient;

namespace Jibril.Services.Reaction
{
    public class ReactionDb
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        public ReactionDb(string table)
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