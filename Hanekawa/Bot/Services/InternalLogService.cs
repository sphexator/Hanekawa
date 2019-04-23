using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Qmmands;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hanekawa.Bot.Services
{
    public class InternalLogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly ILogger<InternalLogService> _logger;

        public InternalLogService(DiscordSocketClient client, CommandService command, ILogger<InternalLogService> logger)
        {
            _client = client;
            _command = command;
            _logger = logger;
            
            _client.Log += Logdiscord;
            _command.CommandExecuted += CommandExecuted;
            _command.CommandErrored += CommandErrored;
        }

        public void LogAction(LogLevel level, string log) => _logger.Log(level, log);

        private Task CommandErrored(ExecutionFailedResult result, ICommandContext context, IServiceProvider provider)
        {
            throw new NotImplementedException();
        }

        private Task CommandExecuted(Command command, CommandResult result, ICommandContext context, IServiceProvider provider)
        {
            throw new NotImplementedException();
        }

        private Task Logdiscord(LogMessage arg)
        {
            throw new NotImplementedException();
        }
    }
}