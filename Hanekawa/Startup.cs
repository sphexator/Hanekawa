using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Qmmands;
using Victoria;
using LogLevel = NLog.LogLevel;

namespace Hanekawa
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            new DatabaseClient(Configuration["connectionString"]);
            
            using (var db = new DbService())
                db.Database.Migrate();

            services.AddControllers();
            services.AddHostedService<Bot.Hanekawa>();
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            }));
            services.AddSingleton(new CommandService(new CommandServiceConfiguration
            {
                DefaultRunMode = RunMode.Parallel,
                CooldownBucketKeyGenerator = (obj, cxt, provider) =>
                {
                    var context = (HanekawaContext) cxt;
                    return context.User.Id;
                }
            }));
            services.AddSingleton<InteractiveService>();
            services.AddSingleton<ColourService>();
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new LavaSocketClient());
            services.AddSingleton(new LavaRestClient(new Configuration
            {
                AutoDisconnect = true,
                SelfDeaf = false,
                LogSeverity = LogSeverity.Info,
                PreservePlayers = true
            }));
            services.AddSingleton<Random>();
            services.AddSingleton<HttpClient>();
            services.UseQuartz(typeof(WarnService));

            var assembly = Assembly.GetAssembly(typeof(Program));
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            for (var i = 0; i < serviceList.Count; i++)
            {
                var x = serviceList[i];
                services.AddSingleton(x);
                //if (x.GetInterfaces().Contains(typeof(INService))) services.AddSingleton(x);
                //else if (x.GetInterfaces().Contains(typeof(INService))) services.AddTransient(x);
                //else if (x.GetInterfaces().Contains(typeof(INService))) services.AddScoped(x);
            }
        }

        public void ConfigureNLog()
        {
            var consoleTarget = new ColoredConsoleTarget
            {
                Name = "Console",
                Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}",
                DetectConsoleAvailable = true
            };
            var fileTarget = new FileTarget
            {
                Name = "File",
                FileName = "${basedir}/logs/${longdate}-log.txt",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            var dbTarget = new DatabaseTarget
            {
                Name = "Database",
                ConnectionString = Configuration["connectionString"],
                DBProvider = "",
                CommandText = "insert into Logs " +
                              "(Timestamp, Level, Message, Logger, CallSite, Exception) " +
                              "values " +
                              "(@Logged, @Level, @Message, @Logger, @Callsite, @Exception)",
                CommandType = CommandType.Text,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("@Logged", Layout.FromString("${date")),
                    new DatabaseParameterInfo("@Level", Layout.FromString("${level}")),
                    new DatabaseParameterInfo("@Message", Layout.FromString("${message}")),
                    new DatabaseParameterInfo("@Logger", Layout.FromString("${logger}")),
                    new DatabaseParameterInfo("@Callsite", Layout.FromString("${callsite}")),
                    new DatabaseParameterInfo("@Exception", Layout.FromString("${exception:tostring}"))
                },
                OptimizeBufferReuse = true
            };

            var config = new LoggingConfiguration();
            config.AddTarget(consoleTarget);
            config.AddTarget(fileTarget);
            config.AddTarget(dbTarget);

            config.AddRuleForAllLevels(consoleTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }
    }
}