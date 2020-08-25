using System;
using System.Threading.Tasks;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Utility
{
    public class VoiceRoleService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;

        public VoiceRoleService(Hanekawa client, IServiceProvider provider)
        {
            _client = client;
            _provider = provider;

            _client.VoiceStateUpdated += VoiceRoleAssignOrRemove;
        }

        private Task VoiceRoleAssignOrRemove(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.NewVoiceState == null)
                {
                    await DisconnectedAsync(e);
                    return;
                }

                if (e.OldVoiceState == null)
                {
                    await JoinedAsync(e);
                    return;
                }

                if (e.OldVoiceState.ChannelId != e.NewVoiceState.ChannelId)
                {
                    await ChangedChannelAsync(e);
                }
            });
            return Task.CompletedTask;
        }

        private async Task DisconnectedAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.VoiceRoles.FindAsync(e.Member.Guild.Id.RawValue, e.OldVoiceState.ChannelId.RawValue);
            if (check == null) return;
            await e.Member.RevokeRoleAsync(e.Member.Guild.GetRole(check.RoleId).Id);
        }

        private async Task JoinedAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.VoiceRoles.FindAsync(e.Member.Guild.Id.RawValue, e.NewVoiceState.ChannelId.RawValue);
            if (check == null) return;
            await e.Member.GrantRoleAsync(e.Member.Guild.GetRole(check.RoleId).Id);
        }

        private async Task ChangedChannelAsync(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var checkOld = await db.VoiceRoles.FindAsync(e.Member.Guild.Id.RawValue, e.OldVoiceState.ChannelId.RawValue);
            var checkNew =
                await db.VoiceRoles.FindAsync(e.Member.Guild.Id.RawValue, e.NewVoiceState.ChannelId.RawValue);
            if (checkOld != null && checkNew != null)
            {
                if (checkOld.RoleId == checkNew.RoleId) return;
                await e.Member.RevokeRoleAsync(e.Member.Guild.GetRole(checkOld.RoleId).Id);
                await e.Member.GrantRoleAsync(e.Member.Guild.GetRole(checkNew.RoleId).Id);
                return;
            }

            if (checkOld != null)
            {
                await e.Member.RevokeRoleAsync(e.Member.Guild.GetRole(checkOld.RoleId).Id);
                return;
            }

            if (checkNew != null)
            {
                await e.Member.GrantRoleAsync(e.Member.Guild.GetRole(checkNew.RoleId).Id);
                return;
            }
        }
    }
}
