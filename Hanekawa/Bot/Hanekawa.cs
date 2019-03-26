using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot
{
    public class Hanekawa : INService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private bool _startUp = false;
        
        public Hanekawa(DiscordSocketClient client, IServiceProvider provider, IConfiguration config)
        {
            _client = client;
            _provider = provider;
            _config = config;
        }

        private void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            var servicelist = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IRequired))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            foreach (var x in servicelist)
            {
                _provider.GetRequiredService(x);
            }

            _startUp = true;
        }
        
        public async Task StartAsync()
        {
            if(!_startUp) Initialize();
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();
        }

        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}