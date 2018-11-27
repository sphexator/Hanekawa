using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.AnimeSimulCast;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Patreon;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Services;
using Hanekawa.Services.Administration;
using Hanekawa.Services.Events;
using Hanekawa.Services.Logging;
using Hanekawa.Services.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa
{
    public class HanekawaBot
    {
        public async Task StartAsync()
        {
            var config = BuildConfig();
            var services = new ServiceCollection();
            services.AddDbContext<DbService>(options => options.UseMySql(config["connectionString"]));
            services.UseQuartz(typeof(EventService));
            services.UseQuartz(typeof(WarnService));
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Error
            }));
            services.AddSingleton(config);
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new PatreonClient(config["patreon"]));
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "db2";
            });

            var assembly = Assembly.GetAssembly(typeof(HanekawaBot));

            var hanakawaServices = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IHanaService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract);
            foreach (var x in hanakawaServices) services.AddSingleton(x);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LogService>();
            provider.GetRequiredService<DiscordLogging>();
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
    }
}