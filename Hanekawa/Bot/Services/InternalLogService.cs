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

            _client.Logger.MessageLogged += Logger_MessageLogged;
            _command.CommandExecutionFailed += CommandErrorLog;
            _command.CommandExecuted += CommandExecuted;

            _castClient.Log += SimulCastClientLog;
        }

        private void Logger_MessageLogged(object sender, MessageLoggedEventArgs e) => _logger.Log(LogSevToLogLevel(e.Severity), e.Exception, e.Message);
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