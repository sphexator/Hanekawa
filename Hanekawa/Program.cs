using System;
using System.Diagnostics;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hanekawa
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try { CreateWebHostBuilder(args).Build().Run(); }
            finally { LogManager.Shutdown(); }
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:61039");
                })
                .ConfigureLogging((e, x) =>
                {
                    x.AddNLog(ConfigureNLog(e.Configuration), NLogAspNetCoreOptions.Default);
                    x.ClearProviders();
                    x.SetMinimumLevel(LogLevel.Information);
                    x.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                })
                .ConfigureDiscordBot<Bot.Hanekawa>((context, bot) =>
                {
                    bot.Token = context.Configuration["token"];
                    bot.UseMentionPrefix = true;
                    bot.Intents = new GatewayIntents(GatewayIntent.Bans |
                                                     GatewayIntent.Guilds |
                                                     GatewayIntent.Emojis |
                                                     GatewayIntent.Integrations |
                                                     GatewayIntent.Webhooks |
                                                     GatewayIntent.Invites |
                                                     GatewayIntent.VoiceStates |
                                                     GatewayIntent.GuildMessages |
                                                     GatewayIntent.GuildReactions |
                                                     GatewayIntent.Members);
                    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
                })
                .UseNLog();

        private static LoggingConfiguration ConfigureNLog(IConfiguration configuration)
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
                ConnectionString = configuration["connectionString"],
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

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, asyncConsoleTarget);
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, asyncDatabaseTarget);
            LogManager.Configuration = config;
            LogManager.ThrowExceptions = Debugger.IsAttached;

            return config;
        }
    }
    
    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory factory) => _logger = factory.CreateLogger(typeof(T).Name);

        public bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, 
            Func<TState, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        public IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}