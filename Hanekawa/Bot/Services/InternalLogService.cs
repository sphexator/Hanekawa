using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Entities;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hanekawa.Bot.Services
{
    public class InternalLogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly ILogger<InternalLogService> _logger;
        private readonly LavaSocketClient _lavaClient;

        public InternalLogService(DiscordSocketClient client, CommandService command, ILogger<InternalLogService> logger, LavaSocketClient lavaClient)
        {
            _client = client;
            _command = command;
            _logger = logger;
            _lavaClient = lavaClient;

            _client.Log += Logdiscord;
            _command.Log += CommandLog;

            _lavaClient.Log += LavaClientLog;
            _lavaClient.OnPlayerUpdated += LavaClientOnPlayerUpdated;
            _lavaClient.OnServerStats += LavaClientOnServerStats;
            _lavaClient.OnSocketClosed += LavaClientOnSocketClosed;
        }

        private Task CommandLog(LogMessage arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        public void LogAction(LogLevel level, string log) => _logger.Log(level, log);

        private Task Logdiscord(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(log.Exception, log.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(log.Exception, log.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(log.Exception, log.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(log.Exception, log.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        private Task LavaClientOnSocketClosed(int code, string reason, bool remote)
        {
            _logger.LogInformation("Victoria", $"Lavasocket closed: {code} | {reason} | {remote}");
            return Task.CompletedTask;
        }

        private Task LavaClientOnServerStats(ServerStats stats)
        {
            _logger.LogInformation("Victoria", $"Uptime: {stats.Uptime}");
            return Task.CompletedTask;
        }

        private Task LavaClientOnPlayerUpdated(LavaPlayer player, LavaTrack track, TimeSpan position)
        {
            _logger.LogInformation("Victoria", $"Player Updated For {player.VoiceChannel.GuildId}: {position}");
            return Task.CompletedTask;
        }

        private Task LavaClientLog(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(log.Exception, log.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(log.Exception, log.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(log.Exception, log.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(log.Exception, log.Message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}