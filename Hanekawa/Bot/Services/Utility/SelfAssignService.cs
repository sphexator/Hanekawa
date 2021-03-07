using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly SemaphoreSlim _lock;

        public SelfAssignService(Hanekawa client, IServiceProvider provider)
        {
            _client = client;
            _provider = provider;
            _lock = new SemaphoreSlim(1, 1);

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
                await _lock.WaitAsync();
                try
                {
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
                }
                finally
                {
                    _lock.Release();
                }
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
                await _lock.WaitAsync();
                try
                {
                    if (user.Roles.ContainsKey(role.Id)) await user.RevokeRoleAsync(role.Id);
                }
                finally
                {
                    _lock.Release();
                }
            });
            return Task.CompletedTask;
        }

        public async Task<List<ulong>> PostAsync(HanekawaCommandContext context, CachedTextChannel channel, DbService db)
        {
            var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == context.Guild.Id.RawValue)
                .OrderByDescending(x => x.Exclusive).ToListAsync();
            if (roles.Count == 0) return null;
            var messages = new List<ulong>();
            var react = new List<LocalCustomEmoji>();
            var reactExt = new List<LocalCustomEmoji>();
            var str = new StringBuilder();
            var strEx = new StringBuilder();
            var counter = 0;
            for (var i = 0; i < roles.Count; i++)
            {
                var x = roles[i];
                var role = context.Guild.GetRole(x.RoleId);
                if (role == null) continue;
                if (!x.Exclusive) counter = 0;
                var msg = LocalCustomEmoji.TryParse(x.EmoteMessageFormat, out var result) 
                    ? $"{result}{role.Mention}" 
                    : $"{role.Mention}";
                if (x.Exclusive)
                {
                    if(result != null) reactExt.Add(result);
                    if (counter.IsDivisible(5)) str.AppendLine($"{msg}");
                    else str.Append($" {msg}");
                }
                else
                {
                    if (result != null) react.Add(result);
                    if (counter.IsDivisible(5)) strEx.AppendLine($"{msg}");
                    else strEx.Append($" {msg}");
                }
            }

            if (str.Length > 0)
            {
                var embed = new LocalEmbedBuilder
                {
                    Title = "Self-assignable roles",
                    Description = str.ToString(),
                    Color = context.ServiceProvider.GetRequiredService<ColourService>().Get(context.Guild.Id.RawValue)
                };
                var rctMsg = await channel.SendMessageAsync(null, false, embed.Build(), LocalMentions.None);
                
                if(react.Count > 0)
                    foreach (var x in react) 
                        await rctMsg.AddReactionAsync(x);
                messages.Add(rctMsg.Id.RawValue);
            }

            if (strEx.Length <= 0) return messages;
            {
                var embed = new LocalEmbedBuilder
                {
                    Title = "Self-assignable roles",
                    Description = strEx.ToString(),
                    Color = context.ServiceProvider.GetRequiredService<ColourService>().Get(context.Guild.Id.RawValue)
                };
                var rctExtMsg = await channel.SendMessageAsync(null, false, embed.Build(), LocalMentions.None);
                
                if(reactExt.Count > 0)
                    foreach (var x in reactExt)
                        await rctExtMsg.AddReactionAsync(x);
                messages.Add(rctExtMsg.Id.RawValue);
            }
            return messages;
        }

        public async Task UpdatePostAsync(HanekawaCommandContext context)
        {

        }
    }
}