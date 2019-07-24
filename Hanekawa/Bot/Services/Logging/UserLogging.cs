using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUpdated(SocketUser before, SocketUser after)
        {
            _ = Task.Run(async () =>
            {
                if (!(before is SocketGuildUser user)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                        if (!cfg.LogAvi.HasValue) return;
                        var channel = user.Guild.GetTextChannel(cfg.LogAvi.Value);
                        if (channel is null) return;
                        var embed = new EmbedBuilder().CreateDefault("", user.Guild.Id);
                        if (before.Username != after.Username)
                        {
                            embed.Title = "Username Change";
                            embed.Description = $"{before} || {before.Id}";
                            embed.AddField(x =>
                            {
                                x.Name = "Old Name";
                                x.Value = $"{before.Username}";
                                x.IsInline = true;
                            });
                            embed.AddField(x =>
                            {
                                x.Name = "New Name";
                                x.Value = $"{after.Username}";
                                x.IsInline = true;
                            });
                        }
                        else if (before.AvatarId != after.AvatarId)
                        {
                            embed.Title = "Avatar Change";
                            embed.Description = $"{before} | {before.Id}";
                            embed.ThumbnailUrl = before.GetAvatar();
                            embed.ImageUrl = after.GetAvatar();
                        }
                        else
                        {
                            return;
                        }

                        await channel.ReplyAsync(embed);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id} for User Updated - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(before.Guild);
                        if (!cfg.LogAvi.HasValue) return;
                        var channel = before.Guild.GetTextChannel(cfg.LogAvi.Value);
                        if (channel == null) return;

                        var embed = new EmbedBuilder().CreateDefault("", before.Guild.Id);
                        embed.Title = $"{after} | {after.Id}";
                        embed.Footer = new EmbedFooterBuilder {IconUrl = after.GetAvatar(), Text = ""};
                        if (before.Nickname != after.Nickname)
                        {
                            embed.Author = new EmbedAuthorBuilder {Name = "Nickname Change"};
                            embed.Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder {Name = "New Nick", Value = after.Nickname ?? after.Username},
                                new EmbedFieldBuilder {Name = "Old Nick", Value = before.Nickname ?? before.Username}
                            };
                        }
                        else if (before.Roles.SequenceEqual(after.Roles))
                        {
                            if (before.Roles.Count < after.Roles.Count)
                            {
                                var roleDiffer = after.Roles
                                    .Where(x => !before.Roles.Contains(x)).Select(x => x.Name);
                                embed.WithAuthor(x => x.WithName("User Role Added"))
                                    .WithDescription(string.Join(", ", roleDiffer));
                            }
                            else if (before.Roles.Count > after.Roles.Count)
                            {
                                var roleDiffer = before.Roles
                                    .Where(x => !after.Roles.Contains(x)).Select(x => x.Name);
                                embed.WithAuthor(x => x.WithName("User Role Removed"))
                                    .WithDescription(string.Join(", ", roleDiffer));
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }

                        await channel.ReplyAsync(embed);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {before.Guild.Id} for Guild Member Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}