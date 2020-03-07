using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<EventPayout> GetOrCreateEventParticipant(this DbService context, CachedMember user)
        {
            var userdata = await context.EventPayouts.FindAsync(user.Guild.Id, user.Id).ConfigureAwait(false);
            if (userdata != null) return userdata;

            var data = new EventPayout
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Amount = 0
            };
            try
            {
                await context.EventPayouts.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.EventPayouts.FindAsync(user.Guild.Id, user.Id).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Board> GetOrCreateBoard(this DbService context, CachedGuild guild, IUserMessage msg)
        {
            var check = await context.Boards.FindAsync(guild.Id, msg.Id).ConfigureAwait(false);
            if (check != null) return check;

            var data = new Board
            {
                GuildId = guild.Id,
                MessageId = msg.Id,
                StarAmount = 0,
                Boarded = null,
                UserId = msg.Author.Id
            };
            try
            {
                await context.Boards.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.Boards.FindAsync(guild.Id, msg.Id).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, CachedGuild guild,
            DateTime time)
        {
            var counter = await context.Suggestions.CountAsync(x => x.GuildId == guild.Id).ConfigureAwait(false);
            int nr;
            if (counter == 0)
                nr = 1;
            else
                nr = counter + 1;

            var data = new Suggestion
            {
                Id = nr,
                GuildId = guild.Id,
                Date = time,
                UserId = user.Id,
                Status = true
            };
            try
            {
                await context.Suggestions.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.Suggestions.FirstOrDefaultAsync(x => x.Date == time).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }
    }
}