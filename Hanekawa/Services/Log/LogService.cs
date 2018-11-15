﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Services.Log
{
    public class LogService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ILogger<LogService> _logger;

        public LogService(DiscordSocketClient client, CommandService commands, ILogger<LogService> logger)
        {
            _client = client;
            _commands = commands;
            _logger = logger;

            _client.Log += LogDiscord;
            _commands.Log += LogCommand;
        }

        private Task LogDiscord(LogMessage message)
        {
            var _ = Task.Run(() =>
            {
                _logger.Log(
                    LogLevelFromSeverity(message.Severity),
                    0,
                    message,
                    message.Exception,
                    (__, ___) => message.ToString(prependTimestamp: false));
            });
            return Task.CompletedTask;
        }

        public void LogAction(LogLevel level, string log, string source)
        {
            _logger.Log(level, log, source);
        }

        private Task LogCommand(LogMessage message)
        {
            var _ = Task.Run(() =>
            {
                // Return an error message for async commands
                if (message.Exception is CommandException command)
                {
                    var __ = _client.GetGuild(431617676859932704).GetTextChannel(512434322360238085)
                        .SendMessageAsync($"Error: {command.Message}\n" +
                                          $"{message.Exception.Message.Truncate(1500)}");
                }

                _logger.Log(
                    LogLevelFromSeverity(message.Severity),
                    0,
                    message,
                    message.Exception,
                    (_1, _2) => message.ToString(prependTimestamp: false));
            });
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
        {
            return (LogLevel) Math.Abs((int) severity - 5);
        }
    }
}