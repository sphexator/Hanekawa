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
            var userdata = await context.EventPayouts.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue).ConfigureAwait(false);
            if (userdata != null) return userdata;

            var data = new EventPayout
            {
                GuildId = user.Guild.Id.RawValue,
                UserId = user.Id.RawValue,
                Amount = 0
            };
            try
            {
                await context.EventPayouts.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.EventPayouts.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Board> GetOrCreateBoard(this DbService context, CachedGuild guild, IMessage msg)
        {
            var check = await context.Boards.FindAsync(guild.Id.RawValue, msg.Id.RawValue).ConfigureAwait(false);
            if (check != null) return check;

            var data = new Board
            {
                GuildId = guild.Id.RawValue,
                MessageId = msg.Id.RawValue,
                StarAmount = 0,
                Boarded = null,
                UserId = msg.Author.Id.RawValue
            };
            try
            {
                await context.Boards.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.Boards.FindAsync(guild.Id.RawValue, msg.Id.RawValue).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, CachedGuild guild,
            DateTime time)
        {
            var counter = await context.Suggestions.CountAsync(x => x.GuildId == guild.Id.RawValue).ConfigureAwait(false);
            var nr = counter == 0 ? 1 : counter + 1;

            var data = new Suggestion
            {
                Id = nr,
                GuildId = guild.Id.RawValue,
                Date = time,
                UserId = user.Id.RawValue,
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