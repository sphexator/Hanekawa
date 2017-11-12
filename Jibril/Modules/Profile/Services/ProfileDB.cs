using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileDB
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.server;
        private readonly string username = DbInfo.username;

        public ProfileDB(string table)
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

        public static void AddProfileURL(IUser user, string url)
        {
            var database = new ProfileDB("hanekawa");
            var str = $"UPDATE exp SET profilepic = '{url}' WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveProfileURL(IUser user)
        {
            var database = new ProfileDB("hanekawa");
            var str = $"UPDATE exp SET profilepic = 'o' WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }
    }
}