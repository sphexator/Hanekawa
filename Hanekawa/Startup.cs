using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Prefix;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Bot.Services.Anime;
using Hanekawa.Bot.Services.Mvp;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Qmmands;
using Quartz;
using ILogger = Disqord.Logging.ILogger;
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
            services.AddControllers();
            services.AddHostedService<RunBot>();
            services.AddHostedService<SimulCastService>();
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddDbContextPool<DbService>(x =>
            {
                x.UseNpgsql("Server=localhost; Port=5432; Database=hanekawa-development; Userid=postgres;Password=1023;"/*Configuration["connectionString"]*/);
                x.EnableDetailedErrors(true);
                x.EnableSensitiveDataLogging(false);
            });
            services.AddSingleton(new Random());
            services.AddSingleton(new HttpClient());
            services.AddSingleton(new ColourService());
            services.UseQuartz(typeof(WarnService));
            services.UseQuartz(typeof(MvpService));

            var assembly = Assembly.GetEntryAssembly();
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            for (var i = 0; i < serviceList.Count; i++)
            {
                var x = serviceList[i];
                services.AddSingleton(x);
            }

            services.AddSingleton(x =>
            {
                var bot = new Bot.Hanekawa(TokenType.Bot, Configuration["token"], new GuildPrefix(x),
                    new DiscordBotConfiguration
                    {
                        MessageCache = new Optional<MessageCache>(new DefaultMessageCache(100)),
                        Logger = new Optional<ILogger>(new DiscordLogger()),
                        CommandServiceConfiguration = new CommandServiceConfiguration
                        {
                            DefaultRunMode = RunMode.Parallel,
                            StringComparison = StringComparison.OrdinalIgnoreCase,
                            CooldownBucketKeyGenerator = (e, context) =>
                            {
                                var ctx = (HanekawaCommandContext) context;
                                return ctx.User.Id.RawValue;
                            }
                        },
                        ProviderFactory = _ => x
                    });
                return bot;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            app.UseHttpsRedirection();
            using var scope = app.ApplicationServices.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            db.Database.Migrate();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });
            NLog.Web.NLogBuilder.ConfigureNLog(ConfigureNLog());

            var assembly = Assembly.GetEntryAssembly();
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IRequired))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            for (var i = 0; i < serviceList.Count; i++) app.ApplicationServices.GetRequiredService(serviceList[i]);
            var scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();
            QuartzExtension.StartCronJob<WarnService>(scheduler, "0 0 13 1/1 * ? *");
            QuartzExtension.StartCronJob<MvpService>(scheduler, "0 0 18 1/1 * ? *");
        }

        private LoggingConfiguration ConfigureNLog()
        {
            var consoleTarget = new ConsoleTarget
            {
                Name = "Console",
                Layout = @"${longdate} | ${level} | ${message} | ${exception}",
                DetectConsoleAvailable = true,
                OptimizeBufferReuse = true,
                AutoFlush = true
            };
            var fileTarget = new FileTarget
            {
                Name = "File",
                FileName = "${basedir}/logs/${shortdate}-log.txt",
                Layout = "${longdate} | ${level} | ${message} | ${exception}",
                OptimizeBufferReuse = true,
                AutoFlush = true,
                ConcurrentWriteAttempts = -1,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveOldFileOnStartup = true,
                ConcurrentWrites = true,
                CreateDirs = true
            };
            var dbTarget = new DatabaseTarget
            {
                Name = "Database",
                ConnectionString = Configuration["connectionString"],
                DBProvider = "Npgsql.NpgsqlConnection,Npgsql",
                CommandText = @"INSERT INTO ""Logs"" " +
                              @"(""TimeStamp"", ""Level"", ""Message"", ""Logger"", ""CallSite"", ""Exception"") " +
                              @"VALUES " +
                              @"(@datetime, @level, @message, @logger, @callsite, @exception)",
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("@datetime", "${longdate}"),
                    new DatabaseParameterInfo("@level", "${level}"),
                    new DatabaseParameterInfo("@message", "${message}"),
                    new DatabaseParameterInfo("@logger", "${logger}"),
                    new DatabaseParameterInfo("@callsite", "${callsite}"),
                    //new DatabaseParameterInfo("@exception", "${exception:format=shortType,message :separator= - }${newline}${exception:format=method}${newline}${exception:format=stackTrace:maxInnerExceptionLevel=5:innerFormat=shortType,message,method}")
                    new DatabaseParameterInfo("@exception", "${exception:format=toString,Data}")
                },
                OptimizeBufferReuse = true
            };

            var asyncConsoleTarget = new AsyncTargetWrapper
            {
                Name = "Async Console Target",
                OptimizeBufferReuse = true,
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                WrappedTarget = consoleTarget,
                TimeToSleepBetweenBatches = 1,
                QueueLimit = 25
            };

            var asyncFileTarget = new AsyncTargetWrapper
            {
                Name = "Async File Target",
                OptimizeBufferReuse = true,
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                WrappedTarget = fileTarget,
                TimeToSleepBetweenBatches = 1,
                QueueLimit = 25
            };

            var asyncDatabaseTarget = new AsyncTargetWrapper
            {
                Name = "Async Database Target",
                OptimizeBufferReuse = true,
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                WrappedTarget = dbTarget,
                TimeToSleepBetweenBatches = 1,
                QueueLimit = 25
            };

            var config = new LoggingConfiguration();
            config.AddTarget(asyncConsoleTarget);
            config.AddTarget(asyncFileTarget);
            config.AddTarget(asyncDatabaseTarget);

            config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncConsoleTarget);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, asyncDatabaseTarget);
            LogManager.Configuration = config;
            LogManager.ThrowExceptions = Debugger.IsAttached;
            
            return config;
        }
    }

    public class RunBot : BackgroundService
    {
        private readonly Bot.Hanekawa _client;
        public RunBot(Bot.Hanekawa client) => _client = client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.AddExtensionAsync(new InteractivityExtension());
            await _client.RunAsync(stoppingToken);
            await Task.Delay(-1, stoppingToken);
        }
    }
}