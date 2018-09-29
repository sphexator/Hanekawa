namespace Hanekawa.Addons.Database
{
    public class DatabaseClient
    {
        public DatabaseClient(string connectionString)
        {
            Config.ConnectionString = connectionString;
        }
    }
}