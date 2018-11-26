using System;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Data
{
    public class Config
    {
        public Config()
        {
        }

        public Config(IConfiguration config)
        {
            GoogleApi = config["perspective"];
            BanApi = config["banlist"];
            ConnectionString = config["connectionString"];
            Console.WriteLine("Config loaded");
        }

        public static string GoogleApi { get; private set; }
        public static string BanApi { get; private set; }
        public static string ConnectionString { get; private set; }
    }
}