using System;
using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Database.Tables.Club;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<Club> CreateClub(this DbService context, CachedUser user, CachedGuild guild,
            string name, DateTimeOffset time)
        {
            var data = new Club
            {
                GuildId = guild.Id,
                Leader = user.Id,
                Name = name,
                CreationDate = time,
                Channel = null,
                Description = null,
                AdMessage = null,
                AutoAdd = false,
                ImageUrl = null,
                Public = false,
                IconUrl = null
            };
            await context.ClubInfos.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.Leader == user.Id).ConfigureAwait(false);
        }
        
        public static async Task<Club> GetClubAsync(this DbService context, CachedMember user, Guid id)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == user.GuildId).ConfigureAwait(false);
            return check;
        }
    }
}
