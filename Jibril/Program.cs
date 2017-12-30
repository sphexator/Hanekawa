using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services;
using Jibril.Services.Automate.PicDump;
using Jibril.Services.Automate.Service;
using Jibril.Services.AutoModerator;
using Jibril.Services.Level;
using Jibril.Services.Logging;
using Jibril.Services.Reaction;
using Jibril.Services.Welcome;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

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
            services.GetRequiredService<PostPictures>();

            var scheduler = services.GetService<IScheduler>();

            //QuartzServicesUtilities.StartSimpleJob<PostPictures>(scheduler, TimeSpan.FromDays(1));
            QuartzServicesUtilities.StartCronJob<PostPictures>(scheduler, "0 50 20 ? * SAT");
            //QuartzServicesUtilities.StartCronJob<BanScheduler>(scheduler, "0 0 14 1/1 * ? *");

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.UseQuartz(typeof(PostPictures));
            services.AddSingleton(_client);
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<LevelingService>();
            services.AddSingleton<WelcomeService>();
            services.AddSingleton<ReactionService>();
            services.AddSingleton<ModerationService>();
            services.AddSingleton<PostPictures>();
            services.AddLogging();
            services.AddSingleton<LogService>();
            services.AddSingleton(_config);
            services.AddSingleton<InteractiveService>();
            services.AddSingleton<QuartzJonFactory>();
            services.AddSingleton<IJobFactory, QuartzJonFactory>();
            return services.BuildServiceProvider();
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