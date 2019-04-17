namespace Hanekawa.Database
{
    public class DatabaseClient
    {
        public DatabaseClient(string connectionString) => Config.ConnectionString = connectionString;
    }

    internal static class Config
    {
        internal static string ConnectionString { get; set; }
    }
}