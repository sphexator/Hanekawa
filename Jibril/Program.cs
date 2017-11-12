using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services;
using Jibril.Services.Automate.PicDump;
using Jibril.Services.AutoModerator;
using Jibril.Services.Level;
using Jibril.Services.Logging;
using Jibril.Services.Reaction;
using Jibril.Services.Welcome;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jibril
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IConfiguration _config;

        private static void Main(string[] args)
        {
            new Program().MainASync().GetAwaiter().GetResult();
        }

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
            services.GetRequiredService<PictureSpam>();

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
                .AddSingleton<PictureSpam>()
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