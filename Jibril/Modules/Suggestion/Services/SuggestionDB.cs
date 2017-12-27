using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Suggestion.Services
{
    public class SuggestionDB
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        public SuggestionDB(string table)
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


        public static void AddSuggestion(IUser user, DateTime now)
        {
            var database = new SuggestionDB("hanekawa");
            var str = $"INSERT INTO suggestion (user_id, date) VALUES ('{user.Id}', '{now}')";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<int> GetSuggestionID(DateTime time)
        {
            var result = new List<int>();
            var database = new SuggestionDB("hanekawa");
            var str = $"SELECT * FROM suggestion WHERE date = '{time}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var suggestNr = (int) reader["id"];
                result.Add(suggestNr);
            }

            database.CloseConnection();
            return result;
        }

        public static void UpdateSuggestion(string msgid, int id)
        {
            var database = new SuggestionDB("hanekawa");
            var str = $"UPDATE suggestion SET msgid = '{msgid}' WHERE id = '{id}'";
            var tablename = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RespondSuggestion(uint casenr, string response, IUser user)
        {
            var database = new SuggestionDB("hanekawa");
            var str = $"UPDATE suggestion SET responduser = '{user.Id}', response = '{response}' WHERE id = '{casenr}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<string> SuggestionMessage(uint casenr)
        {
            var result = new List<string>();
            var database = new SuggestionDB("hanekawa");
            var str = $"SELECT * FROM suggestion WHERE id = '{casenr}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var msgid = (string) reader["msgid"];

                result.Add(msgid);
            }

            database.CloseConnection();
            return result;
        }
    }
}