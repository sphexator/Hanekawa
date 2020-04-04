using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Logging;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Hanekawa.Bot.Services
{
    public class InternalLogService : INService, IRequired
    {
        private readonly AnimeSimulCastClient _castClient;
        private readonly DiscordClient _client;
        private readonly CommandService _command;
        private readonly ILogger<InternalLogService> _logger;

        public InternalLogService(DiscordClient client,
            ILogger<InternalLogService> logger, AnimeSimulCastClient castClient, CommandService command)
        {
            _client = client;
            _logger = logger;
            _castClient = castClient;
            _command = command;

            //_client.Log += LogDiscord;
            _command.CommandExecutionFailed += CommandErrorLog;
            _command.CommandExecuted += CommandExecuted;

            _castClient.Log += SimulCastClientLog;
            /*
            _lavaClient.Log += LavaClientLog;
            _lavaClient.OnPlayerUpdated += LavaClientOnPlayerUpdated;
            _lavaClient.OnServerStats += LavaClientOnServerStats;
            _lavaClient.OnSocketClosed += LavaClientOnSocketClosed;
            */
        }

        public void LogAction(LogLevel l, Exception e, string m) => _logger.Log(l, e, m);

        public void LogAction(LogLevel l, string m) => _logger.Log(l, m);

        private Task SimulCastClientLog(Exception e)
        {
            _logger.Log(LogLevel.Error, e, e.Message);
            return Task.CompletedTask;
        }

        private Task CommandExecuted(CommandExecutedEventArgs e)
        {
            _logger.Log(LogLevel.Information, $"Executed Command {e.Context.Command.Name}");
            return Task.CompletedTask;
        }

        private Task CommandErrorLog(CommandExecutionFailedEventArgs e)
        {
            _logger.Log(LogLevel.Error, e.Result.Exception, $"{e.Result.Reason} - {e.Result.CommandExecutionStep}");
            return Task.CompletedTask;
        }
        /*
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
        */
        private LogLevel LogSevToLogLevel(LogMessageSeverity log)
        {
            switch (log)
            {
                case LogMessageSeverity.Critical:
                    return LogLevel.Critical;
                case LogMessageSeverity.Error:
                    return LogLevel.Error;
                case LogMessageSeverity.Warning:
                    return LogLevel.Warning;
                case LogMessageSeverity.Information:
                    return LogLevel.Information;
                case LogMessageSeverity.Debug:
                    return LogLevel.Trace;
                default:
                    return LogLevel.None;
            }
        }
    }
}