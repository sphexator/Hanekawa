using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database.Tables.Club;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Addons.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<ClubInformation> CreateClub(this DbService context, IUser user, IGuild guild,
            string name, DateTimeOffset time)
        {
            var data = new ClubInformation
            {
                GuildId = guild.Id,
                LeaderId = user.Id,
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
            await context.ClubInfos.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.LeaderId == user.Id);
        }

        public static async Task<ClubInformation> GetClubAsync(this DbService context, int id, IGuild guild)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == guild.Id);
            return check;
        }

        public static async Task<ClubInformation> IsClubLeader(this DbService context, ulong guild, ulong user)
        {
            try
            {
                var leader = await context.ClubInfos.FirstOrDefaultAsync(x =>
                    x.GuildId == guild && x.LeaderId == user);
                return leader;
            }
            catch
            {
                return null;
            }
        }
    }
}
