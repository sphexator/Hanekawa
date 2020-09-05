using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Utility
{
    public class SelfAssignService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;

        public SelfAssignService(Hanekawa client, IServiceProvider provider)
        {
            _client = client;
            _provider = provider;

            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
        }

        private Task ReactionAdded(ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.User.HasValue) return;
                if (e.User.Value.IsBot) return;
                if (!(e.User.Value is CachedMember user)) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateChannelConfigAsync(user.Guild);
                if (!cfg.SelfAssignableChannel.HasValue) return;
                if (cfg.SelfAssignableMessages.Length == 0) return;
                if (e.Channel.Id.RawValue != cfg.SelfAssignableChannel.Value) return;
                if (!cfg.SelfAssignableMessages.Contains(e.Message.Id.RawValue)) return;
                
                var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                    x.GuildId == user.Guild.Id.RawValue && x.EmoteMessageFormat == e.Emoji.MessageFormat);
                if (reaction == null) return;
                var role = user.Guild.GetRole(reaction.RoleId);
                if (role == null) return;
                if (reaction.Exclusive)
                {
                    var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == user.Guild.Id.RawValue && x.Exclusive)
                        .ToListAsync();
                    foreach (var x in roles)
                    {
                        var exclusiveRole = user.Guild.GetRole(x.RoleId);
                        if (exclusiveRole == null) continue;
                        if (user.Roles.Values.Contains(exclusiveRole)) await user.TryRemoveRoleAsync(exclusiveRole);
                    }
                }
                if (!user.Roles.ContainsKey(role.Id)) await user.GrantRoleAsync(role.Id);
            });
            return Task.CompletedTask;
        }

        private Task ReactionRemoved(ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.User.HasValue) return;
                if (e.User.Value.IsBot) return;
                if (!(e.User.Value is CachedMember user)) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateChannelConfigAsync(user.Guild.Id.RawValue);
                if (!cfg.SelfAssignableChannel.HasValue) return;
                if (cfg.SelfAssignableMessages.Length == 0) return;
                if (e.Channel.Id != cfg.SelfAssignableChannel.Value) return;
                if (cfg.SelfAssignableMessages.Contains(e.Message.Id.RawValue)) return;

                var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                    x.GuildId == user.Guild.Id.RawValue && x.EmoteMessageFormat == e.Emoji.MessageFormat);

                if (reaction == null) return;
                var role = user.Guild.GetRole(reaction.RoleId);
                if (role == null) return;
                if (user.Roles.ContainsKey(role.Id)) await user.RevokeRoleAsync(role.Id);
            });
            return Task.CompletedTask;
        }

        public async Task<List<ulong>> PostAsync(HanekawaCommandContext context, CachedTextChannel channel, DbService db)
        {
            var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == context.Guild.Id).ToListAsync();
            var toReturn = new List<ulong>();
            var nonExclusive = new List<SelfAssignAbleRole>();
            for (var i = 0; i < roles.Count;)
            {
                var toAddReaction = new List<LocalCustomEmoji>();
                var message = new StringBuilder();
                for (var j = 0; j < 20;)
                {
                    var line = new StringBuilder();
                    for (var k = 0; k < 5; k++)
                    {
                        if (i >= roles.Count)
                        {
                            i++;
                            j++;
                            continue;
                        }
                        var x = roles[i];
                        var role = context.Guild.GetRole(x.RoleId);
                        if (role == null || !x.Exclusive)
                        {
                            k--;
                            if (!x.Exclusive && role != null) nonExclusive.Add(x);
                            continue;
                        }
                        if (i + 1 >= roles.Count) line.Append($"{role.Mention}");
                        else if (k >= 0 && k < 4) line.Append($"{role.Mention} - ");
                        else if (k == 4) line.Append($"{role.Mention}");
                        if (LocalCustomEmoji.TryParse(x.EmoteMessageFormat, out var result)) toAddReaction.Add(result);
                        i++;
                        j++;
                    }

                    message.AppendLine(line.ToString());
                }
                var embed = new LocalEmbedBuilder
                {
                    Title = "Self-assignable roles",
                    Description = message.ToString(),
                    Color = context.ServiceProvider.GetRequiredService<ColourService>().Get(context.Guild.Id.RawValue)
                };
                var msg = await channel.SendMessageAsync(null, false, embed.Build(), LocalMentions.None);
                for (var j = 0; j < toAddReaction.Count; j++)
                {
                    var reaction = toAddReaction[j];
                    await msg.AddReactionAsync(reaction);
                }
                toReturn.Add(msg.Id.RawValue);
            }

            if (nonExclusive.Count != 0)
            {
                var message = new StringBuilder();
                var toAddReaction = new List<LocalCustomEmoji>();
                for (var i = 0; i < nonExclusive.Count;)
                {
                    var line = new StringBuilder();
                    for (var j = 0; j < 5; j++)
                    {
                        if (i >= roles.Count)
                        {
                            i++;
                            j++;
                            continue;
                        }
                        var x = nonExclusive[i];
                        var role = context.Guild.GetRole(x.RoleId);
                        if (i + 1 >= nonExclusive.Count) line.Append($"{role.Mention}");
                        else if (j >= 0 && j < 4) line.Append($"{role.Mention} - ");
                        else if (j == 4) line.Append($"{role.Mention}");

                        if (LocalCustomEmoji.TryParse(x.EmoteMessageFormat, out var result)) toAddReaction.Add(result);
                        i++;
                    }

                    message.AppendLine(line.ToString());
                }

                var msg = await channel.SendMessageAsync(null, false, new LocalEmbedBuilder
                {
                    Title = "Self-assignable roles",
                    Description = message.ToString(),
                    Color = context.ServiceProvider.GetRequiredService<ColourService>().Get(context.Guild.Id.RawValue)
                }.Build(), LocalMentions.None);

                for (var j = 0; j < toAddReaction.Count; j++)
                {
                    var reaction = toAddReaction[j];
                    await msg.AddReactionAsync(reaction);
                }
                toReturn.Add(msg.Id.RawValue);
            }

            return toReturn;
        }

        public async Task UpdatePostAsync(HanekawaCommandContext context)
        {

        }
    }
}