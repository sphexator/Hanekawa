using System;
using System.Diagnostics;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
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
                LogLevel = LogSeverity.Info,
                ExclusiveBulkDelete = true
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            app.UseHttpsRedirection();

            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageProperties = true,
                CaptureMessageTemplates = true
            });
            ConfigureNLog();
        }

        public void ConfigureNLog()
        {
            var consoleTarget = new ColoredConsoleTarget
            {
                Name = "Console",
                Layout = @"${longdate} | ${level} | ${message} | ${exception}"
            };
            var fileTarget = new FileTarget
            {
                Name = "File",
                FileName = "${basedir}/logs/${shortdate}-log.txt",
                Layout = "${longdate} | ${level} | ${message} | ${exception}"
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

            var config = new LoggingConfiguration();
            config.AddTarget(consoleTarget);
            // config.AddTarget(fileTarget);
            config.AddTarget(dbTarget);

            // var minFileLog = LogLevel.Info;
            var minDbLog = LogLevel.Warn;

            config.AddRuleForAllLevels(consoleTarget);
            // config.AddRule(minFileLog, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, dbTarget);
            
            LogManager.Configuration = config;
            LogManager.ThrowExceptions = Debugger.IsAttached;
        }
    }
}