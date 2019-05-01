using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService
    {
        public async Task PromoteUserAsync(SocketGuildUser user, ClubUser clubUser, ClubInformation clubInfo, DbService db)
        {
            if (clubUser.Rank == 2)
            {
                var leader = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == clubInfo.Id && x.GuildId == user.Guild.Id && x.UserId == clubInfo.LeaderId);
                if (leader == null || clubInfo.LeaderId == 1) return;
                leader.Rank++;
                clubUser.Rank--;
                clubInfo.LeaderId = user.Id;
            }
            else
            {
                clubUser.Rank--;
            }
            await db.SaveChangesAsync();
        }

        public async Task DemoteAsync(SocketGuildUser user, ClubUser clubUser, ClubInformation clubInfo, DbService db)
        {
            if (clubUser.Rank == 3) return;
            clubUser.Rank++;
            await db.SaveChangesAsync();
        }

        public async Task AddUserAsync(SocketGuildUser user, int id, DbService db, ClubConfig cfg = null)
        {
            await db.ClubPlayers.AddAsync(new ClubUser
            {
                ClubId = id,
                GuildId = user.Guild.Id,
                UserId = user.Id,
                JoinDate = DateTimeOffset.UtcNow,
                Rank = 3
            });
            await db.SaveChangesAsync();
        }

        public async Task<bool> RemoveUserAsync(SocketGuildUser user, int id, DbService db, ClubConfig cfg = null)
        {
            if (cfg == null) cfg = await db.GetOrCreateClubConfigAsync(user.Guild);
            var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id && x.GuildId == user.Guild.Id && x.ClubId == id);
            if (clubUser == null) return false;
            var clubInfo =
                await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == user.Guild.Id && x.Id == clubUser.ClubId);
            if (clubUser.Rank == 1)
            {
                var clubMembers = await db.ClubPlayers.Where(x => x.GuildId == user.Guild.Id && x.ClubId == clubUser.ClubId)
                    .ToListAsync();
                if (clubMembers.Count > 1)
                {
                    var officers = clubMembers.Where(x => x.Rank == 2).ToList();
                    var newLeader = officers.Count >= 1 ? officers[_random.Next(officers.Count)] : clubMembers[_random.Next(clubMembers.Count)];

                    newLeader.Rank = 1;
                    clubInfo.LeaderId = newLeader.UserId;
                }
            }
            db.ClubPlayers.Remove(clubUser);
            await RemoveRoleOrChannelPermissions(user, clubInfo, cfg);
            await Disband(user, clubInfo, db);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBlacklist(SocketGuildUser user, SocketGuildUser leader, ClubInformation clubInfo, DbService db, string reason = "N/A")
        {
            var check = await db.ClubBlacklists.FindAsync(clubInfo.Id, user.Guild.Id, user.Id);
            if (check != null) return false;
            await db.ClubBlacklists.AddAsync(new ClubBlacklist
            {
                ClubId = clubInfo.Id,
                GuildId = user.Guild.Id,
                BlackListUser = user.Id,
                IssuedUser = leader.Id,
                Reason = reason,
                Time = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            var club = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id && x.GuildId == user.Guild.Id && x.ClubId == clubInfo.Id);
            if (club != null) await RemoveUserAsync(user, clubInfo.Id, db);
            return true;
        }

        public async Task<bool> RemoveBlacklist(SocketGuildUser user, ClubInformation clubInfo, DbService db)
        {
            var clubUser = await db.ClubBlacklists.FindAsync(clubInfo.Id, user.Guild.Id, user.Id);
            if (clubUser == null) return false;
            db.ClubBlacklists.Remove(clubUser);
            await db.SaveChangesAsync();
            return true;
        }

        private async Task AddRoleOrChannelPermissions(SocketGuildUser user, ClubInformation club, DbService db, ClubConfig cfg = null)
        {
            if (cfg == null) cfg = await db.GetOrCreateClubConfigAsync(user.Guild);
            if (cfg.RoleEnabled && club.Role.HasValue)
                await user.Guild.GetUser(user.Id).TryAddRoleAsync(user.Guild.GetRole(club.Role.Value));
            if (!cfg.RoleEnabled && club.Channel.HasValue)
                await user.Guild.GetTextChannel(club.Channel.Value).AddPermissionOverwriteAsync(user, _allowOverwrite);
        }

        private async Task RemoveRoleOrChannelPermissions(SocketGuildUser user, ClubInformation club, ClubConfig cfg = null)
        {
            try
            {
                if (!club.Channel.HasValue) return;
                if (cfg.RoleEnabled && club.Role.HasValue)
                    await user.Guild.GetUser(user.Id).TryRemoveRoleAsync(user.Guild.GetRole(club.Role.Value));
                if (!cfg.RoleEnabled)
                    await user.Guild.GetTextChannel(club.Channel.Value).RemovePermissionOverwriteAsync(user);
            }
            catch
            {
                // Ignore
            }
        }

        private async Task Disband(SocketGuildUser user, ClubInformation club, DbService db)
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
                if (clubUser.UserId == user.Id)
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
