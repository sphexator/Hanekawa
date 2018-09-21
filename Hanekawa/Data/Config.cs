using Microsoft.Extensions.Configuration;

namespace Hanekawa.Data
{
    public class Config
    {
        public static string GoogleApi { get; private set; }
        public static string BanApi { get; private set; }
        public static string ConnectionString { get; private set; }

        public Config() { }
        public Config(IConfiguration config)
        {
            GoogleApi = config["perspective"];
            BanApi = config["banlist"];
            ConnectionString = config["connectionString"];
        }
    }
}
