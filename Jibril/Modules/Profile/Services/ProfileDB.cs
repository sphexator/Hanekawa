using Discord;
using System;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileDB
    {
        private string _table { get; set; }
        string server = "192.168.10.143";
        string database = "hanekawa";
        string username = "admin";
        string password = "jevel123";
        Boolean POOLING = false;
        private MySqlConnection dbConnection;

        public ProfileDB(string table)
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

        public static void AddProfileURL(IUser user, string url)
        {
            var database = new ProfileDB("hanekawa");
            var str = $"UPDATE exp SET profilepic = '{url}' WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;
        }

        public static void RemoveProfileURL(IUser user)
        {
            var database = new ProfileDB("hanekawa");
            var str = $"UPDATE exp SET profilepic = 'o' WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
            return;
        }
    }
}
