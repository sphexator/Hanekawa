using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
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
                .ConfigureLogging(x =>
                {
                    x.ClearProviders();
                    x.SetMinimumLevel(LogLevel.Information);
                })
                .UseNLog();
    }
}