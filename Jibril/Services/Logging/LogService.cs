using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Common;
using Jibril.Data.Variables;
using Discord.Rest;

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

        private Task UserJoined(SocketGuildUser user)
        {
            var _ =  Task.Run(async () =>
           {
               var content = $"" +
               $"📥 {user.Mention} has joined. (*{user.Id}*)\n" +
               $"Account created: {user.CreatedAt}";
               var embed = EmbedGenerator.FooterEmbed(content, Colours.OKColour, user);
               var channel = user.Guild.GetTextChannel(339380907146477579);
               await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
           });
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser user)
        {
            var _ =  Task.Run(async () =>
            {
                var content = $"" +
                $"📤 {user.Mention} has left. (*{user.Id}*)\n" +
                $"Username: {user.Username}#{user.Discriminator}";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);
                var channel = user.Guild.GetTextChannel(339380907146477579);
                await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private Task Banned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                var content = $"❌ user *bent* \n" +
                $"User: {user.Mention}. (**{user.Id}**)\n" +
                $"Moderator: \n" +
                $"Reason:";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);
                var log = guild.GetTextChannel(339381104534355970);
                await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private Task Unbanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                var content = $"✔ user *unbent* \n" +
                $"user: {user.Mention} (**{user.Id}**)";
                var embed = EmbedGenerator.FooterEmbed(content, Colours.OKColour, user);
                var log = guild.GetTextChannel(339381104534355970);
                await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> optMsg, ISocketMessageChannel ch)
        {
            var _ =  Task.Run(async () =>
            {
                try
                {
                    var msg = (optMsg.HasValue ? optMsg.Value : null) as IUserMessage;
                    if (msg == null)
                        return;

                    var channel = ch as ITextChannel;
                    if (channel == null)
                        return;
                    ITextChannel logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;
                    var user = msg.Author as IUser;
                    if (user.IsBot != true)
                    {
                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Color = new Color(Colours.DefaultColour)
                        };
                        embed.WithDescription($"{msg.Author.Mention} deleted a message in {channel.Mention}:");
                        embed.AddField(efb =>
                        {
                            efb.Name = $"Content:";
                            efb.Value = ($"{msg.Content}");
                            efb.IsInline = false;
                        });
                        embed.WithFooter($"{DateTime.Now}");
                        await logChannel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    // ignored
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg, ISocketMessageChannel channel)
        {
            var _ =  Task.Run(async () =>
            {
                try
                {
                    var msg = (oldMsg.HasValue ? oldMsg.Value : null) as IUserMessage;
                    var chtx = channel as ITextChannel;
                    var user = msg.Author as IUser;
                    ITextChannel logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;

                    if (msg == null) return;
                    if (newMsg == null) return;
                    if (chtx == null) return;
                    if (user.IsBot != true)
                    {
                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Color = new Color(Colours.DefaultColour)
                        };
                        embed.WithDescription($"{msg.Author.Mention} updated a message in {chtx.Mention}");
                        embed.AddField(y =>
                        {
                            y.Name = "Updated Message:";
                            y.Value = $"{newMsg.Content}";
                            y.IsInline = false;
                        });
                        embed.AddField(x =>
                        {
                            x.Name = "Old Message:";
                            x.Value = $"{msg.Content}";
                            x.IsInline = false;
                        });
                        embed.WithFooter($"{DateTime.Now}");
                        await logChannel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
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
