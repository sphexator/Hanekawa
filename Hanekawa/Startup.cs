using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Bot.Services.Administration.Warning;
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
            services.AddSingleton(
                new DiscordBot(TokenType.Bot, Configuration["token"], new DefaultPrefixProvider(),
                    new DiscordBotConfiguration
                    {
                        CommandServiceConfiguration = new CommandServiceConfiguration
                        {
                            DefaultRunMode = RunMode.Sequential,
                            CooldownBucketKeyGenerator = (x, context) =>
                            {
                                var ctx = (HanekawaContext) context;
                                return ctx.User.Id;
                            }
                        }
                    }));
            services.AddDbContextPool<DbService>(x => x.UseNpgsql(Configuration["connectionString"]));
            services.AddSingleton<ColourService>();
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new Random());
            services.AddSingleton(new HttpClient());
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            app.UseHttpsRedirection();
            NLog.Web.NLogBuilder.ConfigureNLog(ConfigureNLog());
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

            config.AddRuleForAllLevels(asyncConsoleTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, asyncDatabaseTarget);

            LogManager.Configuration = config;
            LogManager.ThrowExceptions = Debugger.IsAttached;
            
            return config;
        }
    }
}