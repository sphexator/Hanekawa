using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Services.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Services.Logging
{
    public class LogService
    {
        private readonly CommandService _commands;
        private readonly ILogger _commandsLogger;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _discordLogger;
        private readonly ILoggerFactory _loggerFactory;

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
            var _ = Task.Run(async () =>
            {
                using (var db = new hanekawaContext())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    var content = $"📥 {user.Mention} has joined. (*{user.Id}*)" +
                                  $"\n" +
                                  $"Account created: {user.CreatedAt.Humanize()}";
                    var embed = EmbedGenerator.FooterEmbed(content, $"Username: {user.Username}#{user.Discriminator} - Level: {userdata?.Level ?? 1}", Colours.OkColour, user);
                    var channel = user.Guild.TextChannels.First(x => x.Id == 339380907146477579);
                    await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new hanekawaContext())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    var content = $"📤 {user.Mention} has left. (*{user.Id}*)";
                    var embed = EmbedGenerator.FooterEmbed(content, $"Username: {user.Username}#{user.Discriminator} - Level: {userdata?.Level ?? 1}", Colours.FailColour, user);
                    var channel = user.Guild.TextChannels.First(x => x.Id == 339380907146477579);
                    await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private Task Banned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    await LogEmbedBuilder(guild, user, ActionType.Bent, Colours.FailColour);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task Unbanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    await LogEmbedBuilder(guild, user, ActionType.UnBent, Colours.OkColour);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> optMsg, ISocketMessageChannel ch)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!((optMsg.HasValue ? optMsg.Value : null) is IUserMessage msg))
                        return;
                    if (msg.Author.IsBot) return;
                    if (!(ch is ITextChannel channel)) return;
                    var logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;

                    var author = new EmbedAuthorBuilder
                    {
                        Name = "Message deleted"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        IconUrl = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl(),
                        Text = $"msg ID: {msg.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Color = new Color(Colours.DefaultColour),
                        Author = author,
                        Timestamp = msg.Timestamp,
                        Footer = footer
                    };
                    if (optMsg.HasValue)
                    {
                        embed.WithDescription(
                            $"{optMsg.Value.Author.Mention} deleted a message in {channel.Mention}\n" +
                            $"{msg.Content.Truncate(1800)}");
                    }
                    else
                    {
                        var getMsg = await ch.GetMessageAsync(optMsg.Id);
                        embed.WithDescription($"{getMsg.Author.Mention} deleted a message in {channel.Mention}\n" +
                                              $"{getMsg.Content.Truncate(1800)}");
                    }

                    try
                    {
                        embed.AddField(x =>
                        {
                            x.Name = "File";
                            x.IsInline = false;
                            x.Value = msg.Attachments.FirstOrDefault()?.Url;
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    await Task.Delay(2000);
                    await logChannel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    // ignored
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg,
            ISocketMessageChannel channel)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var msg = (oldMsg.HasValue ? oldMsg.Value : null) as IUserMessage;
                    var user = msg.Author;
                    var logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;

                    if (msg == null) return;
                    if (newMsg == null) return;
                    if (!(channel is ITextChannel chtx)) return;
                    if (user.IsBot != true && oldMsg.Value.Content != newMsg.Content)
                    {
                        var author = new EmbedAuthorBuilder
                        {
                            Name = "Message Updated",
                            IconUrl = newMsg.Author.GetAvatarUrl() ?? newMsg.Author.GetDefaultAvatarUrl()
                        };
                        var footer = new EmbedFooterBuilder
                        {
                            Text = $"MSG ID: {msg.Id}"
                        };
                        var embed = new EmbedBuilder
                        {
                            Color = new Color(Colours.DefaultColour),
                            Author = author,
                            Description = $"{newMsg.Author.Mention} updated a message in {chtx.Mention}",
                            Timestamp = newMsg.Timestamp,
                            Footer = footer
                        };
                        embed.AddField(y =>
                        {
                            y.Name = "Updated Message:";
                            y.Value = $"{newMsg.Content.Truncate(900)}";
                            y.IsInline = false;
                        });
                        embed.AddField(x =>
                        {
                            x.Name = "Old Message:";
                            x.Value = $"{msg.Content.Truncate(900)}";
                            x.IsInline = false;
                        });
                        await Task.Delay(2000);
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
            // Return an error message for async commands
            if (message.Exception is CommandException command)
            {
                Console.WriteLine($"Error: {command.Message}");
                var _ = command.Context.Client.GetApplicationInfoAsync().Result.Owner
                    .SendMessageAsync($"Error: {command.Message}\n" +
                                      $"{command.StackTrace}");
            }

            _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
        {
            return (LogLevel)Math.Abs((int)severity - 5);
        }

        private async Task LogEmbedBuilder(SocketGuild guild, IUser user, string actionType, uint colour,
            int length = 1440)
        {
            using (var db = new hanekawaContext())
            {
                var time = DateTime.Now;
                var caseid = await db.GetOrCreateCaseId(user, time);

                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = $"Case {caseid.Id} | {actionType} | {user.Username}#{user.DiscriminatorValue}"
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = $"ID:{user.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Color = new Color(colour),
                    Author = author,
                    Footer = footer,
                    Timestamp = new DateTimeOffset(DateTime.UtcNow)
                };
                embed.AddField(x =>
                {
                    x.Name = "User";
                    x.Value = $"{user.Mention}";
                    x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = "Moderator";
                    x.Value = $"N/A";
                    x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = "Reason";
                    x.Value = "N/A";
                    x.IsInline = true;
                });

                var log = guild.GetTextChannel(339381104534355970);
                var msg = await log.SendMessageAsync("", false, embed.Build());
                caseid.Msgid = msg.Id.ToString();
                await db.SaveChangesAsync();
            }
        }
    }

    public static class ActionType
    {
        public const string Gagged = "🔇Gagged";
        public const string Ungagged = "🔊UnGagged";
        public const string Bent = "❌Bent";
        public const string UnBent = "✔UnBent";
    }
}