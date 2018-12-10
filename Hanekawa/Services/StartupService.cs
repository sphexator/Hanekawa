using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using Hanekawa.TypeReaders;
using Microsoft.Extensions.Configuration;
using Quartz.Util;

namespace Hanekawa.Services
{
    public class StartupService : IHanaService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _provider;
        private readonly DiscordRestClient _restClient;

        public StartupService(CommandService command, IConfiguration config, IServiceProvider provider,
            DiscordRestClient restClient, DiscordSocketClient client)
        {
            _command = command;
            _config = config;
            _provider = provider;
            _restClient = restClient;
            _client = client;
        }

        public async Task StartupAsync()
        {
            if (_config["token"].IsNullOrWhiteSpace())
            {
                Console.WriteLine("discord token is not specified");
                Console.ReadKey();
                return;
            }

            _command.AddTypeReader(typeof(Emote), new EmoteTypeReader());
            var commands = await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            Console.WriteLine($"{commands.Count()} Commands loaded");

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _restClient.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();
        }
    }
}