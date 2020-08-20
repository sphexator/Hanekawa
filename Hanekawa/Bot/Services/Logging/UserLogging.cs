using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUpdated(UserUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var before = e.OldUser;
                var after = e.NewUser;
                if (!(before is CachedMember user)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var channel = user.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (channel is null) return;

                    var embed = new LocalEmbedBuilder {Color = _colourService.Get(user.Guild.Id.RawValue), Description = ""};
                    if (before.Name != after.Name)
                    {
                        embed.Title = "Username Change";
                        embed.Description = $"{before} || {before.Id.RawValue}";
                        embed.AddField(x =>
                        {
                            x.Name = "Old Name";
                            x.Value = $"{before.Name}";
                            x.IsInline = true;
                        });
                        embed.AddField(x =>
                        {
                            x.Name = "New Name";
                            x.Value = $"{after.Name}";
                            x.IsInline = true;
                        });
                    }
                    else if (before.AvatarHash != after.AvatarHash)
                    {
                        embed.Title = "Avatar Change";
                        embed.Description = $"{before} | {before.Id.RawValue}";
                        embed.ThumbnailUrl = before.GetAvatarUrl();
                        embed.ImageUrl = after.GetAvatarUrl();
                    }
                    else
                    {
                        return;
                    }

                    await channel.ReplyAsync(embed);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for User Updated - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var before = e.OldMember;
                var after = e.NewMember;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(before.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var channel = before.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (channel == null) return;

                    var embed = new LocalEmbedBuilder
                    {
                        Color = _colourService.Get(before.Guild.Id.RawValue),
                        Description = "",
                        Title = $"{after} | {after.Id.RawValue}",
                        Footer = new LocalEmbedFooterBuilder {IconUrl = after.GetAvatarUrl(), Text = $"User ID: {after.Id.RawValue}"}
                    };
                    if (before.Nick != after.Nick)
                    {
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "Nickname Change"};
                        embed.AddField("New Nick", after.Nick ?? after.Name);
                        embed.AddField("Old Nick", before.Nick ?? before.Name);
                    }
                    else if (before.Roles.SequenceEqual(after.Roles))
                    {
                        if (before.Roles.Count < after.Roles.Count)
                        {
                            var roleDiffer = after.Roles
                                .Where(x => !before.Roles.Contains(x)).Select(x => x.Value.Name);
                            embed.WithAuthor(x => x.WithName("User Role Added"))
                                .WithDescription(string.Join(", ", roleDiffer));
                        }
                        else if (before.Roles.Count > after.Roles.Count)
                        {
                            var roleDiffer = before.Roles
                                .Where(x => !after.Roles.Contains(x)).Select(x => x.Value.Name);
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
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {before.Guild.Id.RawValue} for Guild Member Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}