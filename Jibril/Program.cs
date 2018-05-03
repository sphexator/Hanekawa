using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Modules.Audio.Service;
using Jibril.Modules.Club.Services;
using Jibril.Modules.Marriage.Service;
using Jibril.Modules.Report.Service;
using Jibril.Services;
using Jibril.Services.Automate.PicDump;
using Jibril.Services.Automate.Service;
using Jibril.Services.AutoModerator;
using Jibril.Services.INC;
using Jibril.Services.Level;
using Jibril.Services.Logging;
using Jibril.Services.Loot;
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

        private static void Main()
        {
            new Program().MainASync().GetAwaiter().GetResult();
        }

        private async Task MainASync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 1000,
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
            services.GetRequiredService<ModerationService>();
            services.GetRequiredService<PostPictures>();
            services.GetRequiredService<TimedMuteService>();
            services.GetRequiredService<AmInfamous>();
            services.GetRequiredService<LootCrates>();
            services.GetRequiredService<MarriageService>();
            services.GetRequiredService<ReportService>();
            services.GetRequiredService<ClubService>();
            services.GetRequiredService<HungerGames>();

            var scheduler = services.GetService<IScheduler>();

            //QuartzServicesUtilities.StartCronJob<PostPictures>(scheduler, "0 10 18 ? * SAT *");
            QuartzServicesUtilities.StartCronJob<AmInfamous>(scheduler, "0 0 13 ? * MON *");
            QuartzServicesUtilities.StartCronJob<HungerGames>(scheduler, "0 0/1 * 1/1 * ? *");

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
        
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.UseQuartz(typeof(PostPictures));
            services.UseQuartz(typeof(AmInfamous));
            services.AddSingleton(_client);
            services.AddSingleton<MarriageService>();
            services.AddSingleton<ReportService>();
            services.AddSingleton<ClubService>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<AudioService>();
            services.AddSingleton<LevelingService>();
            services.AddSingleton<WelcomeService>();
            services.AddSingleton<ReactionService>();
            services.AddSingleton<ModerationService>();
            services.AddSingleton<LootCrates>();
            services.AddSingleton<HungerGames>();
            services.AddSingleton<AmInfamous>();
            services.AddSingleton<TimedMuteService>();
            services.AddSingleton<PostPictures>();
            services.AddLogging();
            services.AddSingleton<LogService>();
            services.AddSingleton(_config);
            services.AddSingleton<DbInfo>();
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