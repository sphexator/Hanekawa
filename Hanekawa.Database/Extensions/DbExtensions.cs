using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        internal static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter)
        {
            return modelBuilder.UseValueConverterForType(typeof(T), converter);
        }

        internal static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(converter);
                }
            }

            return modelBuilder;
        }
        
        public static async Task<EventPayout> GetOrCreateEventParticipant(this DbService context, CachedMember user)
        {
            var userdata = await context.EventPayouts.FindAsync(user.GuildId, user.Id).ConfigureAwait(false);
            if (userdata != null) return userdata;
        
            var data = new EventPayout
            {
                GuildId = user.GuildId,
                UserId = user.Id,
                Amount = 0
            };
            try
            {
                await context.EventPayouts.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.EventPayouts.FindAsync(user.GuildId, user.Id).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }
        
        public static async Task<Board> GetOrCreateBoardAsync(this DbService context, Snowflake guild, IMessage msg)
        {
            var check = await context.Boards.FindAsync(guild, msg.Id).ConfigureAwait(false);
            if (check != null) return check;
        
            var data = new Board
            {
                GuildId = guild,
                MessageId = msg.Id,
                StarAmount = 0,
                Boarded = null,
                UserId = msg.Author.Id
            };
            try
            {
                await context.Boards.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.Boards.FindAsync(guild, msg.Id).ConfigureAwait(false);
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
            var nr = counter == 0 ? 1 : counter + 1;
        
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