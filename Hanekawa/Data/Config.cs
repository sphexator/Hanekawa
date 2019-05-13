using System;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Data
{
    public class Config : IHanaService, IRequiredService
    {
        public Config(IConfiguration config)
        {
            GoogleApi = config["perspective"];
            BanApi = config["banlist"];
            ConnectionString = config["connectionString"];
            Console.WriteLine("Config loaded");
        }

        public string GoogleApi { get; }
        public string BanApi { get; }
        public string ConnectionString { get; }
    }
}