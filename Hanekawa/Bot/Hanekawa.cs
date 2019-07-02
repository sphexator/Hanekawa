using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot
{
    public class Hanekawa : INService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private bool _startUp;
        
        public Hanekawa(DiscordSocketClient client, IServiceProvider provider, IConfiguration config)
        {
            _client = client;
            _provider = provider;
            _config = config;
        }

        private async Task Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                _provider.GetService<CommandHandlingService>().InitializeAsync();
                var servicelist = assembly.GetTypes()
                    .Where(x => x.GetInterfaces().Contains(typeof(IRequired))
                                && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
                foreach (var x in servicelist)
                {
                    try
                    {
                        _provider.GetRequiredService(x);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            _startUp = true;
        }
        
        public async Task StartAsync()
        {
            if(!_startUp) await Initialize();
            Console.WriteLine("Logging in...");
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            Console.WriteLine("Logged in");
            Console.WriteLine("Starting...");
            await _client.StartAsync();
            Console.WriteLine("Started");
        }

        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}