using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
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
                if (cfg.SelfAssignableMessages.Count > 0) return;
                if (e.Channel.Id.RawValue != cfg.SelfAssignableChannel.Value) return;
                if (!cfg.SelfAssignableMessages.Contains(e.Message.Id.RawValue)) return;
                var message = cfg.SelfAssignableMessages.FirstOrDefault(x => x == e.Message.Id.RawValue);
                var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                    x.GuildId == user.Guild.Id.RawValue && x.EmoteMessageFormat == e.Emoji.MessageFormat);
                if (reaction == null) return;
                var role = user.Guild.GetRole(reaction.RoleId);
                if (role == null) return;
                if (user.Roles.ContainsKey(role.Id)) await user.RevokeRoleAsync(role.Id);
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
                if (cfg.SelfAssignableMessages.Count > 0) return;
                if (e.Channel.Id != cfg.SelfAssignableChannel.Value) return;
                if (cfg.SelfAssignableMessages.Contains(e.Message.Id.RawValue)) return;

                var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                    x.GuildId == user.Guild.Id.RawValue && x.EmoteMessageFormat == e.Emoji.MessageFormat);

                if (reaction == null) return;
                var role = user.Guild.GetRole(reaction.RoleId);
                if (role == null) return;
                if (!user.Roles.ContainsKey(role.Id)) await user.GrantRoleAsync(role.Id);
            });
            return Task.CompletedTask;
        }
    }
}
