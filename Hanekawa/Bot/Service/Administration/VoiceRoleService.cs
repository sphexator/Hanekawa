using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Administration
{
    public class VoiceRoleService : INService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;

        public VoiceRoleService(Hanekawa bot, IServiceProvider provider)
        {
            _bot = bot;
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async ValueTask VoiceStateUpdateAsync(VoiceStateUpdatedEventArgs e)
        {
            if (e.NewVoiceState == null) await DisconnectedAsync(e);
            else if (e.OldVoiceState == null) await JoinedAsync(e);
            else if (e.OldVoiceState.ChannelId != e.NewVoiceState.ChannelId) await ChangedChannelAsync(e);
            else;
        }

        private async ValueTask JoinedAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
        }

        private async ValueTask DisconnectedAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
        }

        private async ValueTask ChangedChannelAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var checkOld = await db.VoiceRoles.FindAsync(e.GuildId, e.OldVoiceState.ChannelId.Value);
            var checkNew =
                await db.VoiceRoles.FindAsync(e.Member.GuildId, e.NewVoiceState.ChannelId.Value);
            var guild = _bot.GetGuild(e.GuildId);
            if (checkOld != null && checkNew != null)
            {
                if (checkOld.RoleId == checkNew.RoleId) return;
                var roles = e.Member.GetRoles().Keys.ToList();
                if(guild.Roles.TryGetValue(checkNew.RoleId, out var newRole) && !roles.Contains(newRole.Id)) roles.Add(newRole.Id);
                if(guild.Roles.TryGetValue(checkOld.RoleId, out var oldRole) && !roles.Contains(oldRole.Id)) roles.Add(oldRole.Id);
                await e.Member.ModifyAsync(x => x.RoleIds = roles);
                return;
            }

            if (checkOld != null)
            {
                if(guild.Roles.TryGetValue(checkOld.RoleId, out var role)) await e.Member.RevokeRoleAsync(role.Id);
                return;
            }

            if (checkNew != null)
            {
                if(guild.Roles.TryGetValue(checkNew.RoleId, out var role)) await e.Member.GrantRoleAsync(role.Id);
                return;
            }
        }
    }
}