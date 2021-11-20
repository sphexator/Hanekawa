using System;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Hanekawa
{
    public static class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:61039");
                })
                .ConfigureLogging((_, x) =>
                {
                    x.ClearProviders();
                    x.SetMinimumLevel(LogLevel.Information);
                    x.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    x.AddSerilog();
                })
                .ConfigureDiscordBot<Bot.Hanekawa>((context, bot) =>
                {
                    bot.Token = context.Configuration["token"];
                    bot.UseMentionPrefix = true;
                    bot.Intents = new GatewayIntents(GatewayIntent.Bans |
                                                     GatewayIntent.Guilds |
                                                     GatewayIntent.EmojisAndStickers |
                                                     GatewayIntent.Integrations |
                                                     GatewayIntent.Webhooks |
                                                     GatewayIntent.Invites |
                                                     GatewayIntent.VoiceStates |
                                                     GatewayIntent.GuildMessages |
                                                     GatewayIntent.GuildReactions |
                                                     GatewayIntent.Members);
                    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
                })
                .UseSerilog();
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