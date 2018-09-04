using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Data;
using Hanekawa.Preconditions;
using Hanekawa.Services;
using Hanekawa.Services.Administration;
using Hanekawa.Services.Anime;
using Hanekawa.Services.Audio;
using Hanekawa.Services.Automate;
using Hanekawa.Services.AutoModerator;
using Hanekawa.Services.Club;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Games.ShipGame;
using Hanekawa.Services.Games.ShipGame.Data;
using Hanekawa.Services.INC;
using Hanekawa.Services.Level;
using Hanekawa.Services.Level.Services;
using Hanekawa.Services.Log;
using Hanekawa.Services.Loot;
using Hanekawa.Services.Profile;
using Hanekawa.Services.Reaction;
using Hanekawa.Services.Scheduler;
using Hanekawa.Services.Welcome;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using SharpLink;

namespace Hanekawa
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IConfiguration _config;
        private LavalinkManager _lavalink;
        private YouTubeService _youTubeService;
        private AnimeSimulCastClient _anime;

        private static void Main() => new Program().MainASync().GetAwaiter().GetResult();

        private async Task MainASync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true
            });
            _config = BuildConfig();
            using (var db = new DbService())
            {
                await db.Database.MigrateAsync();
            }
            _youTubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = _config["googleApi"],
                ApplicationName = GetType().ToString()
            });

            _lavalink = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 80,
                TotalShards = 1
            });

            _anime = new AnimeSimulCastClient();

            var services = ConfigureServices();
            services.GetRequiredService<Config>();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            services.GetRequiredService<LevelingService>();
            services.GetRequiredService<WelcomeService>();
            services.GetRequiredService<BoardService>();
            services.GetRequiredService<WarnService>();
            services.GetRequiredService<NudeScoreService>();
            //services.GetRequiredService<HungerGames>();
            services.GetRequiredService<ShipGameService>();
            //services.GetRequiredService<MvpService>();
            services.GetRequiredService<LootCrates>();
            services.GetRequiredService<SimulCast>();
            services.GetRequiredService<BlackListService>();

            _client.Ready += LavalinkInitiateAsync;
            
            var scheduler = services.GetService<IScheduler>();
            /*
            //QuartzServicesUtilities.StartCronJob<PostPictures>(scheduler, "0 10 18 ? * SAT *");
            QuartzServicesUtilities.StartCronJob<MvpService>(scheduler, "0 0 13 ? * MON *");
            QuartzServicesUtilities.StartCronJob<HungerGames>(scheduler, "0 0 0/6 1/1 * ? *");

            */
            QuartzServicesUtilities.StartCronJob<WarnService>(scheduler, "0 0 13 1/1 * ? *");
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task LavalinkInitiateAsync() => await _lavalink.StartAsync();

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.UseQuartz(typeof(MvpService));
            services.AddSingleton(_client);
            services.AddSingleton(_lavalink);
            services.AddSingleton(_youTubeService);
            services.AddSingleton(_config);
            services.AddSingleton(_anime);

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "db2";
            });

            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<Calculate>();
            services.AddSingleton<ClubService>();
            services.AddSingleton<LevelingService>();
            services.AddSingleton<WelcomeService>();
            services.AddSingleton<BoardService>();
            services.AddSingleton<ModerationService>();
            services.AddSingleton<HungerGames>();
            services.AddSingleton<MvpService>();
            services.AddSingleton<MuteService>();
            services.AddSingleton<WarnService>();
            services.AddSingleton<BlackListService>();
            services.AddSingleton<NudeScoreService>();
            services.AddSingleton<GameStats>();
            services.AddSingleton<ShipGameService>();
            services.AddSingleton<ProfileBuilder>();
            services.AddSingleton<LootCrates>();
            services.AddSingleton<AudioService>();
            services.AddSingleton<RequiredChannel>();
            services.AddSingleton<SimulCast>();
            services.AddLogging();
            services.AddSingleton<LogService>();
            services.AddSingleton<Config>();
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