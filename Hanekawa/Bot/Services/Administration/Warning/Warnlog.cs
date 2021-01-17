using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
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
        public async Task<LocalEmbedBuilder> GetSimpleWarnlogAsync(CachedMember user, DbService db)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var roleList = (from x in user.Roles where x.Value.Name != "@everyone" select x.Value.Name).ToList();
            var roles = string.Join(", ", roleList);
            var warnings = await GetWarnings(user, db);
            var content = "**⮞ User Information**\n" +
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
                          $"Time: {userdata.StatVoiceTime.Humanize(2)} ({userdata.StatVoiceTime})";
            var embed = new LocalEmbedBuilder()
                .Create(content, _colourService.Get(user.Guild.Id.RawValue));
            embed.Author = new LocalEmbedAuthorBuilder
                {IconUrl = user.GetAvatarUrl(), Name = $"{user.Name}#{user.Discriminator} ({user.Id.RawValue})"};
            foreach (var x in warnings) embed.AddField(x);
            return embed;
        }

        public static async Task<List<string>> GetFullWarnlogAsync(CachedMember user, DbService db)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var warns = await db.Warns.Where(x => x.GuildId == user.Guild.Id.RawValue && x.UserId == user.Id.RawValue).ToListAsync();
            var roleList = (from x in user.Roles where x.Value.Name != "@everyone" select x.Value.Name).ToList();
            var roles = string.Join(", ", roleList);
            var result = new List<string>
            {
                "**⮞ User Information**\n" +
                $"Created: {user.CreatedAt.Humanize()} ({user.CreatedAt})\n",
                "**⮞ Member Information**\n" +
                $"Joined: {user.JoinedAt.Humanize()} ({user.JoinedAt})\n" +
                $"Roles: {roles}\n"
            };
            if (userdata.FirstMessage != null)
                result.Add("**⮞ Activity**\n" +
                           $"Last Message: {userdata.LastMessage.Humanize()}\n" +
                           $"First Message: {userdata.FirstMessage.Value.Humanize()}\n");
            else
                result.Add("**⮞ Activity**\n" +
                           $"Last Message: {userdata.LastMessage.Humanize()}\n" +
                           $"First Message: {user.JoinedAt.Humanize()}\n");
            result.Add("**⮞ Voice Session**\n" +
                       $"Amount: {userdata.Sessions}\n" +
                       $"Time: {userdata.StatVoiceTime.Humanize(2)} ({userdata.StatVoiceTime})\n");
            result.Add("**⮞ Warnings**\n" +
                       $"Active Warnings: {warns.Count(x => x.Valid)}\n" +
                       $"Inactive Warnings: {warns.Count(x => !x.Valid)}\n" +
                       $"Total: {warns.Count}\n" +
                       "Next pages contain specific warnings");
            foreach (var x in warns)
            {
                var input = $"**⮞ {x.Id} - {x.Type}**\n" +
                            $"Moderator: {(await user.Guild.GetOrFetchMemberAsync(x.Moderator)).Mention ?? $"{x.Id}"}\n" +
                            $"Reason: {x.Reason}\n";
                if (x.MuteTimer.HasValue)
                    input += $"Mute duration: {x.MuteTimer.Value.Humanize(2)} ({x.MuteTimer.Value})\n";
                input += $"Date: {x.Time.Humanize()} ({x.Time})";
                result.Add(input);
            }

            return result;
        }

        private static async Task<List<LocalEmbedFieldBuilder>> GetWarnings(CachedMember user, DbService db)
        {
            var result = new List<LocalEmbedFieldBuilder>();
            var list = await db.Warns.Where(x => x.GuildId == user.Guild.Id.RawValue && x.UserId == user.Id.RawValue && x.Valid)
                .ToListAsync();
            var count = list.Count;
            if (count > 10) count = 10;
            for (var i = 0; i < count; i++)
            {
                var x = list[i];
                var input = $"Moderator: {(await user.Guild.GetOrFetchMemberAsync(x.UserId)).Mention ?? $"{x.Id}"}\n" +
                            $"Reason: {x.Reason}\n";
                if (x.MuteTimer.HasValue)
                    input += $"Mute duration: {x.MuteTimer.Value.Humanize(2)} ({x.MuteTimer.Value})\n";
                input += $"Date: {x.Time.Humanize()} ({x.Time})";
                result.Add(new LocalEmbedFieldBuilder
                    { Name = $"Warn ID: {x.Id} - {x.Type}", IsInline = true, Value = input.Truncate(999)} );
            }

            return result;
        }
    }
}