using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Services.Common;
using Microsoft.Extensions.Logging;

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

        private Task Post(EmbedBuilder embed, SocketTextChannel ch = null)
        {
            var _ = Task.Run(async () =>
            {
                await ch.SendMessageAsync("", false, embed.Build());
            });
            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                var content = $"" +
                              $"📥 {user.Mention} has joined. (*{user.Id}*)\n" +
                              $"Account created: {user.CreatedAt}";
                var embed = EmbedGenerator.FooterEmbed(content, $"{DateTime.UtcNow}", Colours.OKColour, user);
                var channel = user.Guild.TextChannels.First(x => x.Id == 339380907146477579);
                await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                var content = $"" +
                              $"📤 {user.Mention} has left. (*{user.Id}*)\n" +
                              $"Username: {user.Username}#{user.Discriminator}";
                var embed = EmbedGenerator.FooterEmbed(content, $"{DateTime.UtcNow}", Colours.FailColour, user);
                var channel = user.Guild.TextChannels.First(x => x.Id == 339380907146477579);
                await channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private Task Banned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    ApplyBanScheduler(user);
                    var time = DateTime.Now;
                    AdminDb.AddActionCase(user, time);
                    var caseId = AdminDb.GetActionCaseID(time);
                    var content = $"❌ *bent* \n" +
                                  $"User: {user.Mention}. (**{user.Id}**)";
                    var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseId[0]}", Colours.FailColour, user);
                    embed.AddField(x =>
                    {
                        x.Name = "Moderator";
                        x.Value = "N/A";
                        x.IsInline = true;
                    });
                    embed.AddField(x =>
                    {
                        x.Name = "Reason";
                        x.Value = "N/A";
                        x.IsInline = true;
                    });
                    var log = guild.TextChannels.First(x => x.Id == 339381104534355970);
                    var msg = await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    CaseNumberGenerator.UpdateCase(msg.Id.ToString(), caseId[0]);
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
                    var caseid = CaseNumberGenerator.InsertCaseID(user);
                    var content = $"✔ *unbent* \n" +
                                  $"user: {user.Mention} (**{user.Id}**)";
                    var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseid[0]}", Colours.OKColour, user);
                    embed.AddField(x =>
                    {
                        x.Name = "Moderator";
                        x.Value = "N/A";
                        x.IsInline = true;
                    });
                    embed.AddField(x =>
                    {
                        x.Name = "Reason";
                        x.Value = "N/A";
                        x.IsInline = true;
                    });
                    var log = guild.TextChannels.First(x => x.Id == 339381104534355970);
                    var msg = await log.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    CaseNumberGenerator.UpdateCase(msg.Id.ToString(), caseid[0]);
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
                    var msg = (optMsg.HasValue ? optMsg.Value : null) as IUserMessage;
                    if (msg == null)
                        return;

                    var channel = ch as ITextChannel;
                    if (channel == null)
                        return;
                    var logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;
                    var user = msg.Author;
                    if (user.IsBot != true)
                    {
                        var embed = new EmbedBuilder
                        {
                            Color = new Color(Colours.DefaultColour)
                        };
                        var footer = new EmbedFooterBuilder();
                        embed.WithDescription($"{msg.Author.Mention} deleted a message in {channel.Mention}:");
                        embed.AddField(efb =>
                        {
                            efb.Name = "Content:";
                            efb.Value = $"{msg.Content}";
                            efb.IsInline = false;
                        });
                        footer.WithText($"{DateTime.UtcNow} - msg ID: {msg.Id}");
                        footer.WithIconUrl(optMsg.Value.Author.GetAvatarUrl());
                        embed.WithFooter(footer);
                        await Task.Delay(2000);
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

        private Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg,
            ISocketMessageChannel channel)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var msg = (oldMsg.HasValue ? oldMsg.Value : null) as IUserMessage;
                    var chtx = channel as ITextChannel;
                    var user = msg.Author;
                    var logChannel = _discord.GetChannel(349065172691714049) as ITextChannel;

                    if (msg == null) return;
                    if (newMsg == null) return;
                    if (chtx == null) return;
                    if (user.IsBot != true && oldMsg.Value.Content != newMsg.Content)
                    {
                        var embed = new EmbedBuilder
                        {
                            Color = new Color(Colours.DefaultColour)
                        };
                        var footer = new EmbedFooterBuilder();
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
                        footer.WithText($"{DateTime.UtcNow} - msg ID: {msg.Id}");
                        footer.WithIconUrl(newMsg.Author.GetAvatarUrl());
                        embed.WithFooter(footer);
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
            return (LogLevel) Math.Abs((int) severity - 5);
        }

        private static void ApplyBanScheduler(IUser user)
        {
            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            if (userdata?.Level < 10)
            {
                var banCheck = AdminDb.CheckBanList(user);
                if (banCheck == null)
                {
                    AdminDb.AddBanPerm(user);
                }
                else AdminDb.UpdateBanPerm(user);
            }
            if (userdata?.Level > 10 && userdata.Level < 25)
            {
                var banCheck = AdminDb.CheckBanList(user);
                if (banCheck == null) AdminDb.AddBan(user);
                else AdminDb.UpdateBanPerm(user);
            }
            if (userdata?.Level > 30)
            {
                var banCheck = AdminDb.CheckBanList(user);
                if (banCheck == null) AdminDb.AddBan(user);
                else AdminDb.UpdateBan(user);
            }
        }
    }
}