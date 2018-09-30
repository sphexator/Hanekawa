﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Hanekawa.Addons.AnimeSimulCast;
using Hanekawa.Addons.Database;
using Hanekawa.Handler;
using Hanekawa.Modules.Audio.Service;
using Hanekawa.Preconditions;
using Hanekawa.Services;
using Hanekawa.Services.Administration;
using Hanekawa.Services.Anime;
using Hanekawa.Services.Automate;
using Hanekawa.Services.AutoModerator;
using Hanekawa.Services.Club;
using Hanekawa.Services.Drop;
using Hanekawa.Services.Events;
using Hanekawa.Services.Games.ShipGame;
using Hanekawa.Services.Games.ShipGame.Data;
using Hanekawa.Services.Level;
using Hanekawa.Services.Level.Util;
using Hanekawa.Services.Log;
using Hanekawa.Services.Profile;
using Hanekawa.Services.Reaction;
using Hanekawa.Services.Reliability;
using Hanekawa.Services.Scheduler;
using Hanekawa.Services.Welcome;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Victoria;
using Config = Hanekawa.Data.Config;

namespace Hanekawa
{
    public class Program
    {
        private AnimeSimulCastClient _anime;
        private DiscordSocketClient _client;
        private IConfiguration _config;
        private DatabaseClient _databaseClient;
        private YouTubeService _youTubeService;

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
            _databaseClient = new DatabaseClient(_config["connectionstring"]);
            _youTubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = _config["googleApi"],
                ApplicationName = GetType().ToString()
            });
            _anime = new AnimeSimulCastClient();

            using (var db = new DbService())
            {
                await db.Database.MigrateAsync();
            }

            var services = ConfigureServices();
            services.GetRequiredService<Config>();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            services.GetRequiredService<LevelingService>();
            services.GetRequiredService<WelcomeService>();
            services.GetRequiredService<BoardService>();
            services.GetRequiredService<WarnService>();
            services.GetRequiredService<NudeScoreService>();
            services.GetRequiredService<ShipGameService>();
            services.GetRequiredService<DropService>();
            services.GetRequiredService<SimulCast>();
            services.GetRequiredService<BlackListService>();
            services.GetRequiredService<ReliabilityService>();
            services.GetRequiredService<EventService>();
            services.GetRequiredService<LavalinkInitialize>();

            var scheduler = services.GetService<IScheduler>();

            QuartzServicesUtilities.StartCronJob<EventService>(scheduler, "0 0 10 1/1 * ? *");
            QuartzServicesUtilities.StartCronJob<WarnService>(scheduler, "0 0 13 1/1 * ? *");
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.UseQuartz(typeof(EventService));
            services.UseQuartz(typeof(WarnService));
            services.AddSingleton(_client);
            services.AddSingleton(_youTubeService);
            services.AddSingleton(_config);
            services.AddSingleton(_anime);

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "db2";
            });

            services.AddSingleton<Lavalink>();
            services.AddSingleton<LavalinkInitialize>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<Calculate>();
            services.AddSingleton<ClubService>();
            services.AddSingleton<LevelingService>();
            services.AddSingleton<WelcomeService>();
            services.AddSingleton<BoardService>();
            services.AddSingleton<ModerationService>();
            services.AddSingleton<MvpService>();
            services.AddSingleton<MuteService>();
            services.AddSingleton<WarnService>();
            services.AddSingleton<EventService>();
            services.AddSingleton<BlackListService>();
            services.AddSingleton<NudeScoreService>();
            services.AddSingleton<GameStats>();
            services.AddSingleton<ShipGameService>();
            services.AddSingleton<ProfileBuilder>();
            services.AddSingleton<DropService>();
            services.AddSingleton<AudioService>();
            //services.AddSingleton<PlaylistService>();
            services.AddSingleton<RequiredChannel>();
            services.AddSingleton<ReliabilityService>();
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