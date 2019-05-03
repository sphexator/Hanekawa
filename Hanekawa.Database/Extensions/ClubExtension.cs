using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database.Tables.Club;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
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
            await context.ClubInfos.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.LeaderId == user.Id).ConfigureAwait(false);
        }

        public static async Task<ClubInformation> GetClubAsync(this DbService context, SocketGuildUser user, int id)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == user.Guild.Id).ConfigureAwait(false);
            return check;
        }
    }
}
