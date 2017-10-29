using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using Jibril.Services;
using Jibril.Services.Logging;
using Jibril.Services.Level;
using Jibril.Services.Welcome;
using Jibril.Services.Reaction;
using Jibril.Services.AutoModerator;

namespace Jibril
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainASync().GetAwaiter().GetResult();
        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task MainASync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 100
            });
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            services.GetRequiredService<LevelingService>();
            services.GetRequiredService<WelcomeService>();
            services.GetRequiredService<ReactionService>();
            services.GetRequiredService<ModerationService>();

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<LevelingService>()
                .AddSingleton<WelcomeService>()
                .AddSingleton<ReactionService>()
                .AddSingleton<ModerationService>()
                .AddLogging()
                .AddSingleton<LogService>()
                .AddSingleton(_config)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
    }
}