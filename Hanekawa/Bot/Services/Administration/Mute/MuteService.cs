using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Moderation;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService : INService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        private readonly OverwritePermissions _denyOverwrite =
            new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny,
                attachFiles: PermValue.Deny);

        public MuteService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            foreach (var x in _db.MuteTimers)
            {
                TimeSpan after;
                if (x.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow) after = TimeSpan.FromMinutes(2);
                else after = x.Time - DateTime.UtcNow;
                StartUnMuteTimer(x.GuildId, x.UserId, after);
            }
        }

        public async Task<bool> TimedMute(SocketGuildUser user, SocketGuildUser staff, TimeSpan after)
        {
            await Mute(user).ConfigureAwait(false);
            var unMuteAt = DateTime.UtcNow + after;
            await _db.MuteTimers.AddAsync(new MuteTimer
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Time = unMuteAt
            });
            await _db.SaveChangesAsync();
            StartUnMuteTimer(user.Guild.Id, user.Id, after);
            //TODO: Connect to discord logs
            return true;
        }

        private async Task<bool> Mute(SocketGuildUser user)
        {
            var role = await GetMuteRoleAsync(user.Guild);
            var check = await user.TryAddRoleAsync(role as SocketRole);
            if (!check) return false;
            _ = ApplyPermissions(user.Guild, role);
            await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            return true;
        }

        private async Task<IRole> GetMuteRoleAsync(SocketGuild guild)
        {
            var cfg = await _db.GetOrCreateAdminConfigAsync(guild);
            IRole role;
            if (!cfg.MuteRole.HasValue)
            {
                role = await CreateRole(guild);
                cfg.MuteRole = role.Id;
                await _db.SaveChangesAsync();
            }
            else role = guild.GetRole(cfg.MuteRole.Value);

            return role;
        }

        private async Task<IRole> CreateRole(SocketGuild guild)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "mute");
            if (role == null) return await guild.CreateRoleAsync("Mute", GuildPermissions.None);
            return role;
        }

        private async Task ApplyPermissions(SocketGuild guild, IRole role)
        {
            foreach (var x in guild.TextChannels)
            {
                if (x.PermissionOverwrites.Select(z => z.Permissions).Contains(_denyOverwrite)) continue;
                await x.AddPermissionOverwriteAsync(role, _denyOverwrite)
                    .ConfigureAwait(false);

                await Task.Delay(200).ConfigureAwait(false);
            }
        }
    }
}
