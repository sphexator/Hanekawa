using Microsoft.Extensions.Configuration;

namespace Jibril.Data
{
    public class Config
    {
        public string GoogleApi { get; private set; }
        public string BanApi { get; private set; }
        public string ConnectionString { get; private set; }

        public Config() { }

        public Config(IConfiguration config)
        {
            GoogleApi = config["perspective"];
            BanApi = config["banlist"];
            ConnectionString = config["connectionString"];
        }
    }
}
