using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Qmmands;
using Victoria;
using Victoria.Entities;

namespace Hanekawa.Bot.Services
{
    public class InternalLogService : INService, IRequired
    {
        private readonly AnimeSimulCastClient _castClient;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly LavaSocketClient _lavaClient;
        private readonly ILogger<InternalLogService> _logger;

        public InternalLogService(DiscordSocketClient client, CommandService command,
            ILogger<InternalLogService> logger, LavaSocketClient lavaClient, AnimeSimulCastClient castClient)
        {
            _client = client;
            _command = command;
            _logger = logger;
            _lavaClient = lavaClient;
            _castClient = castClient;

            _client.Log += LogDiscord;
            _command.CommandErrored += CommandErrorLog;
            _command.CommandExecuted += CommandExecuted;

            _castClient.Log += SimulCastClientLog;

            _lavaClient.Log += LavaClientLog;
            _lavaClient.OnPlayerUpdated += LavaClientOnPlayerUpdated;
            _lavaClient.OnServerStats += LavaClientOnServerStats;
            _lavaClient.OnSocketClosed += LavaClientOnSocketClosed;
        }

        public void LogAction(LogLevel l, Exception e, string m) => _logger.Log(l, e, m);

        private Task SimulCastClientLog(Exception e)
        {
            _logger.Log(LogLevel.Error, e, e.Message);
            return Task.CompletedTask;
        }

        private Task CommandExecuted(CommandExecutedEventArgs e)
        {
            _logger.Log(LogLevel.Information, $"Executed Command {e.Result.Command.Name}");
            return Task.CompletedTask;
        }

        private Task CommandErrorLog(CommandErroredEventArgs e)
        {
            _logger.Log(LogLevel.Error, e.Result.Exception, $"{e.Result.Reason} - {e.Result.CommandExecutionStep}");
            return Task.CompletedTask;
        }

        private Task LogDiscord(LogMessage log)
        {
            _logger.Log(LogSevToLogLevel(log.Severity), log.Exception, log.Message);
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
            _logger.Log(LogSevToLogLevel(log.Severity), log.Exception, log.Message);
            return Task.CompletedTask;
        }

        private LogLevel LogSevToLogLevel(LogSeverity log)
        {
            switch (log)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Verbose:
                    return LogLevel.Trace;
                case LogSeverity.Debug:
                    return LogLevel.Trace;
                default:
                    return LogLevel.None;
            }
        }
    }
}