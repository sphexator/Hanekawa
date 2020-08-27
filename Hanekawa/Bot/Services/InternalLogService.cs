using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Logging;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using NLog;
using Qmmands;
using ILogger = Disqord.Logging.ILogger;
using Logger = NLog.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hanekawa.Bot.Services
{
    public class InternalLogService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly Logger _logger;

        public InternalLogService(Hanekawa client)
        {
            _client = client;
            _logger = LogManager.GetCurrentClassLogger();

            _client.Logger.Logged += DisqordLogger;
            _client.CommandExecutionFailed += CommandErrorLog;
            _client.CommandExecuted += CommandExecuted;
        }

        private void DisqordLogger(object sender, LogEventArgs e) => _logger.Log(LogSevToNLogLevel(e.Severity), e.Exception, e.Message);
        public void LogAction(LogLevel l, Exception e, string m) => _logger.Log(LogLvlToNLogLvl(l), e, m);

        public void LogAction(LogLevel l, string m) => _logger.Log(LogLvlToNLogLvl(l), m);

        private Task SimulCastClientLog(Exception e)
        {
            _logger.Log(NLog.LogLevel.Error, e, e.Message);
            return Task.CompletedTask;
        }

        private Task CommandExecuted(CommandExecutedEventArgs e)
        {
            _logger.Log(NLog.LogLevel.Info, $"Executed Command {e.Context.Command.Name}");
            return Task.CompletedTask;
        }

        private Task CommandErrorLog(CommandExecutionFailedEventArgs e)
        {
            _logger.Log(NLog.LogLevel.Error, e.Result.Exception, $"{e.Result.Reason} - {e.Result.CommandExecutionStep}");
            _ = Task.Run(async () =>
            {
                if (!(e.Context is DiscordCommandContext context)) return;
                Command command;
                Module module = null;
                var response = new StringBuilder();
                switch (e.Result as IResult)
                {
                    case ChecksFailedResult err:
                        command = err.Command;
                        module = err.Module;
                        response.AppendLine("The following check(s) failed:");
                        foreach (var (check, result) in err.FailedChecks) response.AppendLine($"[{check}]: `{result.Reason}`");
                        break;
                    case TypeParseFailedResult err:
                        command = err.Parameter.Command;
                        response.AppendLine(err.Reason);
                        break;
                    case ArgumentParseFailedResult err:
                        command = err.Command;
                        response.AppendLine($"The syntax of the command `{command.FullAliases[0]}` was wrong.");
                        break;
                    case OverloadsFailedResult err:
                        command = err.FailedOverloads.First().Key;
                        response.AppendLine($"I can't find any valid overload for the command `{command.Name}`.");
                        foreach (var overload in err.FailedOverloads) response.AppendLine($" -> `{overload.Value.Reason}`");
                        break;
                    case ParameterChecksFailedResult err:
                        command = err.Parameter.Command;
                        module = err.Parameter.Command.Module;
                        response.AppendLine("The following parameter check(s) failed:");
                        foreach (var (check, error) in err.FailedChecks)
                            response.AppendLine($"[`{check.Parameter.Name}`]: `{error}`");
                        break;
                    case ExecutionFailedResult err:
                        switch (err.Exception)
                        {
                            case HttpRequestException _:
                                response.AppendLine("I am missing the required permissions to perform this action");
                                break;
                            default:
                                response.AppendLine("Something went wrong...");
                                break;
                        }

                        break;
                    case CommandNotFoundResult _:
                        var list = new List<Tuple<Qmmands.Command, int>>();
                        var commands = _client.GetAllCommands();
                        foreach (var x in commands)
                            for (var i = 0; i < x.Aliases.Count; i++)
                            {
                                var alias = x.Aliases.ElementAt(i);
                                alias.FuzzyMatch(e.Context.Alias, out var score);
                                list.Add(new Tuple<Command, int>(x, score));
                            }

                        var reCmd = list.OrderByDescending(x => x.Item2).FirstOrDefault();
                        if (reCmd == null) return;
                        response.AppendLine(
                            $"Did you mean **{reCmd.Item1.Name}** ? (command: {_client.CurrentUser.Mention} {reCmd.Item1.FullAliases.FirstOrDefault()})");
                        break;
                    default:
                        response.AppendLine("Something went wrong...");
                        break;
                }

                if (response.Length == 0) return;
                await context.Channel.ReplyAsync(response.ToString(), Color.Red);
                var msg = new StringBuilder();
                msg.AppendLine($"Error in guild: {context.Guild.Id.RawValue}");
                msg.AppendLine($"Command failed: {e.Result.Command.Name}");
                msg.AppendLine($"Invoked by: {context.User.Id.RawValue}");
                msg.AppendLine($"Reason: {e.Result.Reason}");
                msg.AppendLine($"Response: {response}");
                msg.AppendLine(e.Result.Exception.Message);
                await _client.GetGuild(431617676859932704).GetTextChannel(523165903219720232).SendMessageAsync(msg.ToString().Truncate(1900));
            });
            return Task.CompletedTask;
        }

        private NLog.LogLevel LogSevToNLogLevel(LogSeverity log) =>
            log switch
            {
                LogSeverity.Critical => NLog.LogLevel.Fatal,
                LogSeverity.Error => NLog.LogLevel.Error,
                LogSeverity.Warning => NLog.LogLevel.Warn,
                LogSeverity.Information => NLog.LogLevel.Info,
                LogSeverity.Trace => NLog.LogLevel.Trace,
                LogSeverity.Debug => NLog.LogLevel.Debug,
                _ => NLog.LogLevel.Off
            };

        private NLog.LogLevel LogLvlToNLogLvl(LogLevel log) =>
            log switch
            {
                LogLevel.Critical => NLog.LogLevel.Fatal,
                LogLevel.Error => NLog.LogLevel.Error,
                LogLevel.Warning => NLog.LogLevel.Warn,
                LogLevel.Information => NLog.LogLevel.Info,
                LogLevel.Trace => NLog.LogLevel.Trace,
                LogLevel.Debug => NLog.LogLevel.Debug,
                _ => NLog.LogLevel.Off
            };
    }

    public class DiscordLogger : ILogger, INService, IRequired
    {
        private readonly Logger _logger;

        public DiscordLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Dispose() { }

        public void Log(object sender, LogEventArgs e) => _logger.Log(SevToLogLevel(e.Severity), e.Exception, e.Message);

        public event EventHandler<LogEventArgs> Logged;

        private static NLog.LogLevel SevToLogLevel(LogSeverity log) =>
            log switch
            {
                LogSeverity.Critical => NLog.LogLevel.Fatal,
                LogSeverity.Error => NLog.LogLevel.Error,
                LogSeverity.Warning => NLog.LogLevel.Warn,
                LogSeverity.Information => NLog.LogLevel.Info,
                LogSeverity.Trace => NLog.LogLevel.Trace,
                LogSeverity.Debug => NLog.LogLevel.Debug,
                _ => NLog.LogLevel.Off
            };
    }
}