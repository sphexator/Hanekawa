using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services;
using Jibril.Services.Automate.Service;
using Jibril.Services.AutoModerator;
using Jibril.Services.INC;
using Jibril.Services.Level;
using Jibril.Services.Logging;
using Jibril.Services.Reaction;
using Jibril.Services.Welcome;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Jibril
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IConfiguration _config;

        private static void Main()
        {
            new Program().MainASync().GetAwaiter().GetResult();
        }

        private async Task MainASync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true
            });
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<DbInfo>();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            services.GetRequiredService<LevelingService>();
            services.GetRequiredService<WelcomeService>();
            services.GetRequiredService<ReactionService>();
            services.GetRequiredService<HungerGames>();

            /*
            var scheduler = services.GetService<IScheduler>();

            //QuartzServicesUtilities.StartCronJob<PostPictures>(scheduler, "0 10 18 ? * SAT *");
            QuartzServicesUtilities.StartCronJob<MvpService>(scheduler, "0 0 13 ? * MON *");
            QuartzServicesUtilities.StartCronJob<HungerGames>(scheduler, "0 0 0/6 1/1 * ? *");
            */

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.UseQuartz(typeof(MvpService));
            services.AddSingleton(_client);
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<LevelingService>();
            services.AddSingleton<WelcomeService>();
            services.AddSingleton<ReactionService>();
            services.AddSingleton<ModerationService>();
            services.AddSingleton<HungerGames>();
            services.AddSingleton<MvpService>();
            services.AddLogging();
            services.AddSingleton<LogService>();
            services.AddSingleton(_config);
            services.AddSingleton<DbInfo>();
            services.AddSingleton<InteractiveService>();
            services.AddSingleton<QuartzJonFactory>();
            services.AddSingleton<IJobFactory, QuartzJonFactory>();
            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
    }
}