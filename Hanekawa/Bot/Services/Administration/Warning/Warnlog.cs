using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Administration.Warning
{
    public partial class WarnService
    {
        public async Task<EmbedBuilder> GetSimpleWarnlogAsync(SocketGuildUser user, DbService db)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var roleList = (from x in user.Roles where x.Name != "@everyone" select x.Name).ToList();
            var roles = string.Join(", ", roleList);
            var content = "**⮞ User Information**\n" +
                          $"Status: {user.GetStatus()}\n" +
                          $"{user.GetGame()}\n" +
                          $"Created: {user.CreatedAt.Humanize()} ({user.CreatedAt})\n" +
                          "\n" +
                          "**⮞ Member Information**\n" +
                          $"Joined: {user.JoinedAt.Humanize()} ({user.JoinedAt})\n" +
                          $"Roles: {roles}\n" +
                          "\n" +
                          "**⮞ Activity**\n" +
                          $"Last Message: {userdata.LastMessage.Humanize()} \n" +
                          $"First Message: {userdata.FirstMessage.Humanize()} \n" +
                          "\n" +
                          "**⮞ Session**\n" +
                          $"Amount: {userdata.Sessions}\n" +
                          $"Time: {userdata.StatVoiceTime.Humanize()} ({userdata.StatVoiceTime})";
            var embed = new EmbedBuilder()
                .CreateDefault(content, user.Guild.Id);
            embed.Author = new EmbedAuthorBuilder { IconUrl = user.GetAvatar(), Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Id})" };
            embed.Fields = await GetWarnings(user, db);
            return embed;
        }

        public async Task<List<string>> GetFullWarnlogAsync(SocketGuildUser user, DbService db)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var warns = await db.Warns.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id).ToListAsync();
            var roleList = (from x in user.Roles where x.Name != "@everyone" select x.Name).ToList();
            var roles = string.Join(", ", roleList);
            var result = new List<string>();
            result.Add("*⮞ User Information**\n" +
                       $"Status: {user.GetStatus()}\n" +
                       $"{user.GetGame()}\n" +
                       $"Created: {user.CreatedAt.Humanize()} ({user.CreatedAt})\n");
            result.Add("**⮞ Member Information**\n" +
                       $"Joined: {user.JoinedAt.Humanize()} ({user.JoinedAt})\n" +
                       $"Roles: {roles}\n");
            if (userdata.FirstMessage != null)
                result.Add("**⮞ Activity**\n" +
                           $"Last Message: {userdata.LastMessage.Humanize()}\n" +
                           $"First Message: {userdata.FirstMessage.Value.Humanize()}\n");
            else result.Add("**⮞ Activity**\n" +
                            $"Last Message: {userdata.LastMessage.Humanize()}\n" +
                            $"First Message: {user.JoinedAt.Humanize()}\n");
            result.Add("**⮞ Voice Session**\n" +
                       $"Amount: {userdata.Sessions}\n" +
                       $"Time: {userdata.StatVoiceTime.Humanize()} ({userdata.StatVoiceTime})\n");
            result.Add("**⮞ Warnings**\n" +
                       $"Active Warnings: {warns.Count(x => x.Valid)}\n" +
                       $"Inactive Warnings: {warns.Count(x => !x.Valid)}\n" +
                       $"Total: {warns.Count}\n" +
                       $"Next pages contain specific warnings");
            foreach (var x in warns)
            {
                var input = $"{x.Id} - {x.Type}\n" +
                            $"Moderator: {user.Guild.GetUser(x.UserId).Mention ?? $"{x.Id}"}\n" +
                            $"Reason: {x.Reason}\n";
                if (x.MuteTimer.HasValue)
                    input += $"Mute duration: {x.MuteTimer.Value.Humanize()} ({x.MuteTimer.Value})\n";
                input += $"Date: {x.Time.Humanize()} ({x.Time})";
                result.Add(input);
            }
            return result;
        }

        private async Task<List<EmbedFieldBuilder>> GetWarnings(SocketGuildUser user, DbService db)
        {
            var result = new List<EmbedFieldBuilder>();
            var list = await db.Warns.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id && x.Valid).ToListAsync();
            var count = list.Count;
            if (count > 10) count = 10;
            for (var i = 0; i < count; i++)
            {
                var x = list[i];
                var input = $"Moderator: {user.Guild.GetUser(x.UserId).Mention ?? $"{x.Id}"}\n" +
                            $"Reason: {x.Reason}\n";
                if (x.MuteTimer.HasValue)
                    input += $"Mute duration: {x.MuteTimer.Value.Humanize()} ({x.MuteTimer.Value})\n";
                input += $"Date: {x.Time.Humanize()} ({x.Time})"; ;
                result.Add(new EmbedFieldBuilder
                    { Name = $"Warn ID: {x.Id} - {x.Type}", IsInline = true, Value = input.Truncate(999) });
            }

            return result;
        }
    }
}