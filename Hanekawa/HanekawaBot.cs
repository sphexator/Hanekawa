﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Hanekawa.Addons.AnimeSimulCast;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Patreon;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Services;
using Hanekawa.Services.Administration;
using Hanekawa.Services.Events;
using Hanekawa.Services.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace Hanekawa
{
    public class HanekawaBot
    {
        public async Task StartAsync()
        {
            var config = BuildConfig();

            var databaseClient = new DatabaseClient(config["connectionString"]);
            var services = new ServiceCollection();
            services.UseQuartz(typeof(EventService));
            services.UseQuartz(typeof(WarnService));
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            }));
            services.AddSingleton(new CommandService(
                new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                }));
            services.AddSingleton(config);
            services.AddLogging();
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new PatreonClient(config["patreon"]));
            services.AddSingleton<Lavalink>();
            services.AddSingleton<InteractiveService>();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "db2";
            });

            var assembly = Assembly.GetAssembly(typeof(HanekawaBot));

            var hanakawaServices = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IHanaService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            foreach (var x in hanakawaServices) services.AddSingleton(x);
            var provider = services.BuildServiceProvider();
            ConfigureLogging(provider);

            using (var db = new DbService()) await db.Database.MigrateAsync(); 
            
            foreach (var x in hanakawaServices) provider.GetRequiredService(x);

            var scheduler = provider.GetService<IScheduler>();
            QuartzServicesUtilities.StartCronJob<EventService>(scheduler, "0 0 10 1/1 * ? *");
            QuartzServicesUtilities.StartCronJob<WarnService>(scheduler, "0 0 13 1/1 * ? *");
            await provider.GetRequiredService<StartupService>().StartupAsync();
            await Task.Delay(-1);
        }

        private static IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        private static void ConfigureLogging(IServiceProvider provider)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            NLog.LogManager.LoadConfiguration("nlog.config");
        }
    }
}