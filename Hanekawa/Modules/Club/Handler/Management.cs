using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.Config.Guild;

namespace Hanekawa.Modules.Club.Handler
{
    public class Management : IHanaService
    {
        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow, sendMessages: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        public async Task PromoteUserAsync(DbService db, IGuildUser user, ClubUser clubUser, ClubInformation club)
        {
            if (clubUser.Rank == 2)
            {
                var leader = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.GuildId && x.UserId == club.LeaderId);
                if (leader == null) return;
                leader.Rank++;
                clubUser.Rank--;
                club.LeaderId = user.Id;
            }
            else
            {
                clubUser.Rank--;
            }

            await db.SaveChangesAsync();
        }

        public async Task DemoteUserAsync(DbService db, IGuildUser user, ClubUser clubUser, ClubInformation club)
        {
            if (clubUser.Rank == 3) return;
            clubUser.Rank++;
            await db.SaveChangesAsync();
        }

        public async Task AddUserAsync(DbService db, IGuildUser user, int clubId, ClubConfig cfg = null) =>
            await AddUserAsync(db, user, user.Guild, clubId, cfg).ConfigureAwait(false);

        public async Task
            AddUserAsync(DbService db, IUser user, IGuild guild, ClubUser clubId, ClubConfig cfg = null) =>
            await AddUserAsync(db, user, guild, clubId.ClubId, cfg).ConfigureAwait(false);

        public async Task AddUserAsync(DbService db, IGuildUser user, ClubInformation clubId, ClubConfig cfg = null) =>
            await AddUserAsync(db, user, user.Guild, clubId.Id, cfg).ConfigureAwait(false);

        public async Task AddUserAsync(DbService db, IGuildUser user, ClubUser clubUser, ClubConfig cfg = null) =>
            await AddUserAsync(db, user, user.Guild, clubUser.ClubId, cfg).ConfigureAwait(false);

        public async Task AddUserAsync(DbService db, IUser user, IGuild guild, int id, ClubConfig cfg = null)
        {
            var data = new ClubUser
            {
                ClubId = id,
                GuildId = guild.Id,
                JoinDate = DateTimeOffset.UtcNow,
                Rank = 3,
                UserId = user.Id
            };
            await db.ClubPlayers.AddAsync(data);
            await db.SaveChangesAsync();
            await AddRoleOrChannelPermissions(db, user, guild, await db.ClubInfos.FindAsync(id), cfg);
        }

        public async Task<ClubInformation> RemoveUserAsync(DbService db, IGuildUser user, int clubId, ClubConfig cfg = null) =>
            await RemoveUserAsync(db, user, user.Guild, clubId, cfg).ConfigureAwait(false);

        public async Task<ClubInformation> RemoveUserAsync(DbService db, IUser user, IGuild guild, ClubUser clubId,
            ClubConfig cfg = null) =>
            await RemoveUserAsync(db, user, guild, clubId.ClubId, cfg).ConfigureAwait(false);

        public async Task<ClubInformation>
            RemoveUserAsync(DbService db, IGuildUser user, ClubInformation clubId, ClubConfig cfg = null) =>
            await RemoveUserAsync(db, user, user.Guild, clubId.Id, cfg).ConfigureAwait(false);

        public async Task<ClubInformation> RemoveUserAsync(DbService db, IGuildUser user, ClubUser clubUser, ClubConfig cfg = null) =>
            await RemoveUserAsync(db, user, user.Guild, clubUser.ClubId, cfg).ConfigureAwait(false);

        public async Task<ClubInformation> RemoveUserAsync(DbService db, IUser user, IGuild guild, int clubId,
            ClubConfig cfg = null)
        {
            if (cfg == null) cfg = await db.GetOrCreateClubConfigAsync(guild);
            var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id && x.GuildId == guild.Id && x.ClubId == clubId);
            if (clubUser == null) return null;
            db.ClubPlayers.Remove(clubUser);
            var clubInfo =
                await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.Id == clubUser.ClubId);
            await RemoveRoleOrChannelPermissions(db, user, guild, clubInfo, cfg);
            await DisbandClub(db, clubInfo, user);
            await db.SaveChangesAsync();
            return clubInfo;
        }

        public async Task<bool> BlackListUserAsync(DbService db, IGuildUser blacklistUser, IGuildUser leader,
            int clubId, string reason = "N/A")
        {
            var check = await db.ClubBlacklists.FindAsync(clubId, blacklistUser.GuildId, blacklistUser.Id);
            if (check != null) return false;
            await db.ClubBlacklists.AddAsync(new ClubBlacklist
            {
                ClubId = clubId,
                GuildId = blacklistUser.GuildId,
                BlackListUser = blacklistUser.Id,
                IssuedUser = leader.Id,
                Reason = reason,
                Time = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            var club = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == blacklistUser.Id && x.GuildId == blacklistUser.GuildId && x.ClubId == clubId);
            if (club != null) await RemoveUserAsync(db, blacklistUser, clubId);
            return true;
        }

        public async Task<bool> RemoveBlackListUserAsync(DbService db, IGuildUser blacklistUser, int clubId)
        {
            var user = await db.ClubBlacklists.FindAsync(clubId, blacklistUser.GuildId, blacklistUser.Id);
            if (user == null) return false;
            db.ClubBlacklists.Remove(user);
            await db.SaveChangesAsync();
            return true;
        }

        private async Task AddRoleOrChannelPermissions(DbService db, IUser user, IGuild guild, ClubInformation club,
            ClubConfig cfg)
        {
            if (cfg == null) cfg = await db.GetOrCreateClubConfigAsync(guild);
            if (cfg.RoleEnabled && club.Role.HasValue)
                await (await guild.GetUserAsync(user.Id)).TryAddRoleAsync(guild.GetRole(club.Role.Value));
            if (!cfg.RoleEnabled && club.Channel.HasValue)
                await (await guild.GetTextChannelAsync(club.Channel.Value)).AddPermissionOverwriteAsync(user,
                    _allowOverwrite);
        }

        private async Task RemoveRoleOrChannelPermissions(DbService db, IUser user, IGuild guild, ClubInformation club,
            ClubConfig cfg)
        {
            if (!club.Channel.HasValue) return;
            if (cfg.RoleEnabled && club.Role.HasValue)
                await (await guild.GetUserAsync(user.Id)).TryRemoveRoleAsync(guild.GetRole(club.Role.Value));
            if (!cfg.RoleEnabled)
                await (await guild.GetTextChannelAsync(club.Channel.Value)).RemovePermissionOverwriteAsync(user);
        }

        private async Task DisbandClub(DbService db, ClubInformation club, IUser user)
        {
            var clubMembers = await db.ClubPlayers.Where(x => x.ClubId == club.Id).ToListAsync();
            if (clubMembers.Count == 0)
            {
                db.ClubInfos.Remove(club);
            }

            if (clubMembers.Count == 1)
            {
                if (clubMembers.First().UserId == user.Id)
                {
                    db.ClubInfos.Remove(club);
                }
            }
        }
    }
}