using Microsoft.Extensions.Configuration;

namespace Jibril.Data.Variables
{
    public class DbInfo
    {
        public static string Server { get; private set; }
        public static string Username { get; private set; }
        public static string Password { get; private set; }

        public static string DbNorm { get; private set; }
        public static string DbFleet { get; private set; }
        public static string DbWarn { get; private set; }

        public DbInfo(IConfiguration config)
        {
            var config1 = config;

            Server = config1["dbServer"];
            Username = config1["dbUsername"];
            Password = config1["dbPassword"];
            DbNorm = config1["dbNorm"];
            DbFleet = config1["dbFleet"];
            DbWarn = config1["dbWarn"];
        }
    }
}