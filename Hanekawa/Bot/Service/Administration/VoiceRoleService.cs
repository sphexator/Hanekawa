using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.Administration
{
    public abstract class VoiceRoleService : DiscordClientService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;

        protected VoiceRoleService(ILogger<VoiceRoleService> logger, Hanekawa bot, IServiceProvider provider) : base(logger, bot)
        {
            _bot = bot;
            _provider = provider;
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            if (e.NewVoiceState == null) await DisconnectedAsync(e);
            else if (e.OldVoiceState == null) await JoinedAsync(e);
            else if (e.OldVoiceState.ChannelId != e.NewVoiceState.ChannelId) await ChangedChannelAsync(e);
        }

        protected override async ValueTask OnRoleDeleted(RoleDeletedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var role = await db.VoiceRoles.FirstOrDefaultAsync(x => x.GuildId == e.GuildId && x.RoleId == e.RoleId);
            if (role == null) return;
            db.VoiceRoles.Remove(role);
            await db.SaveChangesAsync();
            Logger.LogInformation("Removed voice role {RoleName} in {GuildId} as it got deleted", e.Role.Name, e.GuildId);
        }

        private async ValueTask JoinedAsync(VoiceStateUpdatedEventArgs e)
        {
            if (!e.NewVoiceState.ChannelId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var newChannel = await db.VoiceRoles.FindAsync(e.Member.GuildId, e.NewVoiceState.ChannelId.Value);
            if (newChannel == null) return;
            if (!_bot.GetGuild(e.GuildId).Roles.TryGetValue(newChannel.RoleId, out var role)) return;
            await e.Member.GrantRoleAsync(role.Id);
            Logger.LogInformation("Added {RoleName} to {UserId} in {GuildId}", role.Name, e.Member.Id, e.GuildId);
        }

        private async ValueTask DisconnectedAsync(VoiceStateUpdatedEventArgs e)
        {
            if (!e.OldVoiceState.ChannelId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var oldChannel = await db.VoiceRoles.FindAsync(e.GuildId, e.OldVoiceState.ChannelId.Value);
            if (oldChannel == null) return;
            if (!_bot.GetGuild(e.GuildId).Roles.TryGetValue(oldChannel.RoleId, out var role)) return; 
            await e.Member.RevokeRoleAsync(role.Id);
            Logger.LogInformation("Removed {RoleName} to {UserId} in {GuildId}", role.Name, e.Member.Id, e.GuildId);
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
                Logger.LogInformation("Updated voice roles to {UserId} in {GuildId}", e.Member.Id, e.GuildId);
                return;
            }
            if (checkOld != null)
            {
                if (!guild.Roles.TryGetValue(checkOld.RoleId, out var role)) return;
                await e.Member.RevokeRoleAsync(role.Id);
                Logger.LogInformation("Removed {RoleName} to {UserId} in {GuildId}", role.Name, e.Member.Id, e.GuildId);
                return;
            }
            if (checkNew != null)
            {
                if(guild.Roles.TryGetValue(checkNew.RoleId, out var role)) await e.Member.GrantRoleAsync(role.Id);
            }
        }
    }
}