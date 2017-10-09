using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Common;
using Jibril.Data.Variables;

namespace Jibril.Services.Logging
{
    public class LogService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _discordLogger;
        private readonly ILogger _commandsLogger;

        public LogService(DiscordSocketClient discord, CommandService commands, ILoggerFactory loggerFactory)
        {
            _discord = discord;
            _commands = commands;

            _loggerFactory = ConfigureLogging(loggerFactory);
            _discordLogger = _loggerFactory.CreateLogger("discord");
            _commandsLogger = _loggerFactory.CreateLogger("commands");

            _discord.Log += LogDiscord;
            _commands.Log += LogCommand;

            _discord.UserBanned += Banned;
            _discord.UserUnbanned += Unbanned;
            _discord.UserJoined += UserJoined;
            _discord.UserLeft += UserLeft;
            _discord.MessageDeleted += MessageDeleted;
            _discord.MessageUpdated += MessageUpdated;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            await Task.Run(async () =>
           {
               var content = $"" +
               $"📥 {user.Mention} has joined. (**{user.Id}**)\n" +
               $"Account created: {user.CreatedAt}";
               var embed = EmbedGenerator.FooterEmbed(content, Colours.OKColour, user);
               var channel = user.Guild.GetTextChannel(339380907146477579);
               await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
           });
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            await Task.Run(async () =>
            {
                var content = $"" +
                $"📤 {user.Mention} has left.\n" +
                $"Username: {user.Username}#{user.Discriminator}";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);
                var channel = user.Guild.GetTextChannel(339380907146477579);
                await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
        }

        private async Task Banned(SocketUser user, SocketGuild guild)
        {
            await Task.Run(async () =>
            {
                var content = $"" +
                $"❌ {user.Username}#{user.Discriminator} got *bent*. (**{user.Id}**)";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);
                var log = guild.GetTextChannel(339381104534355970);
                await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
        }

        private async Task Unbanned(SocketUser user, SocketGuild guild)
        {
            await Task.Run(async () =>
            {
                var content = $"" +
                $"❕ {user.Username}#{user.Discriminator} got *bent*. (**{user.Id}**)";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.OKColour, user);
                var log = guild.GetTextChannel(339381104534355970);
                await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            await Task.Run(() =>
            {

            });
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg, ISocketMessageChannel channel)
        {
            await Task.Run(() =>
            {

            });
        }

        private ILoggerFactory ConfigureLogging(ILoggerFactory factory)
        {
            factory.AddConsole();
            return factory;
        }

        private Task LogDiscord(LogMessage message)
        {
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));
    }
}
