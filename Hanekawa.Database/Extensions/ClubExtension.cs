using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Tables.Club;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<ClubInformation> CreateClub(this DbService context, CachedUser user, CachedGuild guild,
            string name, DateTimeOffset time)
        {
            var data = new ClubInformation
            {
                GuildId = guild.Id.RawValue,
                LeaderId = user.Id.RawValue,
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
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id.RawValue && x.LeaderId == user.Id.RawValue).ConfigureAwait(false);
        }

        public static async Task<ClubInformation> GetClubAsync(this DbService context, CachedMember user, int id)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == user.GuildId.RawValue).ConfigureAwait(false);
            return check;
        }
    }
}
