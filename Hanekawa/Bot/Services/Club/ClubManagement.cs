﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService
    {
        public static async Task PromoteUserAsync(CachedMember user, ClubUser clubUser, ClubInformation clubInfo,
            DbService db)
        {
            if (clubUser.Rank == 2)
            {
                var leader = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == clubInfo.Id && x.GuildId == user.Guild.Id.RawValue && x.UserId == clubInfo.LeaderId);
                if (leader == null || clubInfo.LeaderId == 1) return;
                leader.Rank++;
                clubUser.Rank--;
                clubInfo.LeaderId = user.Id.RawValue;
            }
            else
            {
                clubUser.Rank--;
            }

            await db.SaveChangesAsync();
        }

        public static async Task DemoteAsync(ClubUser clubUser, DbService db)
        {
            if (clubUser.Rank == 3) return;
            clubUser.Rank++;
            await db.SaveChangesAsync();
        }

        public async Task AddUserAsync(CachedMember user, int id, DbService db)
        {
            await db.ClubPlayers.AddAsync(new ClubUser
            {
                ClubId = id,
                GuildId = user.Guild.Id.RawValue,
                UserId = user.Id.RawValue,
                JoinDate = DateTimeOffset.UtcNow,
                Rank = 3
            });
            await db.SaveChangesAsync();
            _log.LogAction(LogLevel.Information, $"(Club Service) Added {user.Id.RawValue} to club {id} in {user.Guild.Id.RawValue}");
        }

        public async Task<bool> RemoveUserAsync(CachedUser user, CachedGuild guild, int id, DbService db, ClubConfig cfg = null)
        {
            if (cfg == null) cfg = await db.GetOrCreateClubConfigAsync(guild);
            var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id.RawValue && x.GuildId == guild.Id.RawValue && x.ClubId == id);
            if (clubUser == null) return false;
            var clubInfo =
                await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id.RawValue && x.Id == clubUser.ClubId);
            if (clubUser.Rank == 1)
            {
                var clubMembers = await db.ClubPlayers
                    .Where(x => x.GuildId == guild.Id.RawValue && x.ClubId == clubUser.ClubId)
                    .ToListAsync();
                if (clubMembers.Count > 1)
                {
                    var officers = clubMembers.Where(x => x.Rank == 2).ToList();
                    var newLeader = officers.Count >= 1
                        ? officers[_random.Next(officers.Count)]
                        : clubMembers[_random.Next(clubMembers.Count)];
                    _log.LogAction(LogLevel.Information, $"(Club Service) Replaced club leader from club id {clubInfo.Id} in {guild.Id.RawValue} from {clubUser.UserId} to {newLeader.Id}");
                    newLeader.Rank = 1;
                    clubInfo.LeaderId = newLeader.UserId;
                }
            }

            db.ClubPlayers.Remove(clubUser);
            await RemoveRoleOrChannelPermissions(user, guild, clubInfo, cfg);
            await Disband(user, clubInfo, db);
            await db.SaveChangesAsync();
            _log.LogAction(LogLevel.Information, $"(Club Service) Removed {clubUser.UserId} from {clubInfo.Id} in {guild.Id.RawValue}");
            return true;
        }

        public async Task<bool> AddBlacklist(CachedUser user, CachedUser leader, CachedGuild guild, ClubInformation clubInfo,
            DbService db, string reason = "N/A")
        {
            var check = await db.ClubBlacklists.FindAsync(clubInfo.Id, guild.Id.RawValue, user.Id.RawValue);
            if (check != null) return false;
            await db.ClubBlacklists.AddAsync(new ClubBlacklist
            {
                ClubId = clubInfo.Id,
                GuildId = guild.Id.RawValue,
                BlackListUser = user.Id.RawValue,
                IssuedUser = leader.Id.RawValue,
                Reason = reason,
                Time = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            var club = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id.RawValue && x.GuildId == guild.Id.RawValue && x.ClubId == clubInfo.Id);
            if (club != null) await RemoveUserAsync(user, guild, clubInfo.Id, db);
            _log.LogAction(LogLevel.Information, $"(Club Service) Added blacklist on user {user.Id.RawValue} in club id {clubInfo.Id} in guild {guild.Id.RawValue}");
            return true;
        }

        public async Task<bool> RemoveBlacklist(CachedUser user, CachedGuild guild, ClubInformation clubInfo, DbService db)
        {
            var clubUser = await db.ClubBlacklists.FindAsync(clubInfo.Id, guild.Id.RawValue, user.Id.RawValue);
            if (clubUser == null) return false;
            db.ClubBlacklists.Remove(clubUser);
            await db.SaveChangesAsync();
            _log.LogAction(LogLevel.Information, $"(Club Service) Removed blacklist on user {user.Id.RawValue} in club id {clubInfo.Id} in guild {guild.Id.RawValue}");
            return true;
        }

        private async Task AddRoleOrChannelPermissions(CachedMember user, ClubInformation club, DbService db,
            ClubConfig cfg = null)
        {
            cfg ??= await db.GetOrCreateClubConfigAsync(user.Guild);
            if (cfg.RoleEnabled && club.Role.HasValue)
                await (await user.Guild.GetOrFetchMemberAsync(user.Id.RawValue) as CachedMember).TryAddRoleAsync(user.Guild.GetRole(club.Role.Value));
            if (!cfg.RoleEnabled && club.Channel.HasValue)
                await user.Guild.GetTextChannel(club.Channel.Value).AddOrModifyOverwriteAsync(new LocalOverwrite(user.Id.RawValue, OverwriteTargetType.Member, _allowOverwrite));
        }

        private static async Task RemoveRoleOrChannelPermissions(CachedUser user, CachedGuild guild, ClubInformation club,
            ClubConfig cfg = null)
        {
            try
            {
                if (!club.Channel.HasValue) return;
                if (cfg.RoleEnabled && club.Role.HasValue)
                    await (await guild.GetOrFetchMemberAsync(user.Id.RawValue) as CachedMember).TryRemoveRoleAsync(guild.GetRole(club.Role.Value));
                if (!cfg.RoleEnabled)
                    await guild.GetTextChannel(club.Channel.Value).DeleteOverwriteAsync(user.Id.RawValue, RestRequestOptions.FromReason("Club Removal"));
            }
            catch
            {
                // Ignore
            }
        }

        private static async Task Disband(CachedUser user, ClubInformation club, DbService db)
        {
            var clubMembers = await db.ClubPlayers.Where(x => x.ClubId == club.Id).ToListAsync();
            if (clubMembers.Count == 0)
            {
                club.AdMessage = null;
                club.AutoAdd = false;
                club.Channel = null;
                club.Description = null;
                club.IconUrl = null;
                club.ImageUrl = null;
                club.LeaderId = 1;
                club.Public = false;
                club.Role = null;
                club.Name = "Disbanded";
                db.Update(club);
                await db.SaveChangesAsync();
            }

            if (clubMembers.Count == 1)
            {
                var clubUser = clubMembers.FirstOrDefault();
                if (clubUser == null) return;
                if (clubUser.UserId == user.Id.RawValue)
                {
                    club.AdMessage = null;
                    club.AutoAdd = false;
                    club.Channel = null;
                    club.Description = null;
                    club.IconUrl = null;
                    club.ImageUrl = null;
                    club.LeaderId = 1;
                    club.Public = false;
                    club.Role = null;
                    club.Name = "Disbanded";
                    db.Update(club);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}