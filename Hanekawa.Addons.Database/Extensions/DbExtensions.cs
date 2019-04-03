using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Tables.BoardConfig;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Addons.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<EventPayout> GetOrCreateEventParticipant(this DbService context, SocketGuildUser user)
        {
            var userdata = await context.EventPayouts.FindAsync(user.Guild.Id, user.Id);
            if (userdata != null) return userdata;

            var data = new EventPayout
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Amount = 0
            };
            try
            {
                await context.EventPayouts.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.EventPayouts.FindAsync(user.Guild.Id, user.Id);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Board> GetOrCreateBoard(this DbService context, IGuild guild, IUserMessage msg)
        {
            var check = await context.Boards.FindAsync(guild.Id, msg.Id);
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
                await context.Boards.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.Boards.FindAsync(guild.Id, msg.Id);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.Suggestions.CountAsync(x => x.GuildId == guild.Id);
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
                await context.Suggestions.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.Suggestions.FirstOrDefaultAsync(x => x.Date == time);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<QuestionAndAnswer> CreateQnA(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.QuestionAndAnswers.CountAsync(x => x.GuildId == guild.Id);
            int nr;
            if (counter == 0)
                nr = 1;
            else
                nr = counter + 1;

            var data = new QuestionAndAnswer
            {
                Id = nr,
                GuildId = guild.Id,
                Date = time,
                UserId = user.Id,
                Status = true
            };
            try
            {
                await context.QuestionAndAnswers.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.QuestionAndAnswers.FirstOrDefaultAsync(x => x.Date == time);
            }
            catch
            {
                return data;
            }
        }
    }
}