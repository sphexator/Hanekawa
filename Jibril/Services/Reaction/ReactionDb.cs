using Jibril.Services.Reaction.List;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Reaction
{
    public class ReactionDb
    {
        private string _table { get; set; }
        string server = "192.168.10.143";
        string database = "hanekawa";
        string username = "admin";
        string password = "jevel123";
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public ReactionDb(string table)
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


        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";

        public static void InsertReactionMessage(string msgid, string channelid, int counter)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"INSERT INTO reaction (msgid, chid, counter) VALUES ('{msgid}', '{channelid}', '{counter}')";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static List<ReactionList> ReactionData(string msgid)
        {
            var result = new List<ReactionList>();
            var database = new ReactionDb("hanekawa");
            var str = $"SELECT * FROM reaction WHERE msgid = {msgid}";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var messageid = (string)reader["msgid"];
                var chid = (string)reader["chid"];
                var counter = (int)reader["counter"];
                var sent = (string)reader["sent"];

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
            return;
            
        }

        public static void RemoveReaction(string msgid)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"UPDATE reaction SET counter = counter - '1' WHERE msgid = '{msgid}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static void ReactionMsgPosted(string msgid)
        {
            var database = new ReactionDb("hanekawa");
            var str = $"UPDATE reaction SET sent = 'yes' WHERE msgid = '{msgid}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
            return;
        }
    }
}
