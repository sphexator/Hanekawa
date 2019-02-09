using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Club.Handler
{
    public class Management : IHanaService
    {
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

        public async Task AddUserAsync(DbService db, IGuildUser user, ClubUser clubUser) =>
            await AddUserAsync(db, user, clubUser.ClubId).ConfigureAwait(false);

        public async Task AddUserAsync(DbService db, IGuildUser user, int id)
        {
            var data = new ClubUser
            {
                ClubId = id,
                GuildId = user.GuildId,
                JoinDate = DateTimeOffset.UtcNow,
                Rank = 3,
                UserId = user.Id
            };
            await db.ClubPlayers.AddAsync(data);
            await db.SaveChangesAsync();
        }

        public async Task RemoveUserAsync(DbService db, IGuildUser user, ClubUser clubUser) =>
            await RemoveUserAsync(db, user, clubUser.ClubId).ConfigureAwait(false);

        public async Task RemoveUserAsync(DbService db, IUser user, ClubUser clubUser) =>
            await RemoveUserAsync(db, user, clubUser.ClubId).ConfigureAwait(false);

        public async Task RemoveUserAsync(DbService db, IUser user, int clubId) =>
            await RemoveUserAsync(db, user, clubId).ConfigureAwait(false);

        public async Task<string> RemoveUserAsync(DbService db, IGuildUser user, int clubId)
        {
            var clubUser = await db.ClubPlayers.FindAsync(user.Id, user.GuildId, clubId);
            if (clubUser == null) return $"Can't remove {user.Mention}.";
            db.ClubPlayers.Remove(clubUser);
            await db.SaveChangesAsync();
            return null;
        }

        public async Task BlackListUserAsync(IGuildUser user, int id)
        {

        }
    }
}
