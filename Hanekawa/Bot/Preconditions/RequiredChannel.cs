using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    public class RequiredChannel : CheckAttribute, INService
    {
        private static ConcurrentDictionary<ulong, bool> IgnoreAll { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private static ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ChannelEnable { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (!(_ is DiscordCommandContext context)) return CheckResult.Unsuccessful("woopsie command context wrong :)");
            if (context.Member.Permissions.ManageGuild)
                return CheckResult.Successful;

            var ignoreAll = IgnoreAll.TryGetValue(context.Guild.Id.RawValue, out var status);
            if (!ignoreAll) status = await UpdateIgnoreAllStatus(context);

            var pass = status ? EligibleChannel(context, true) : EligibleChannel(context);

            switch (pass)
            {
                case true:
                    return CheckResult.Successful;
                case false:
                    return CheckResult.Unsuccessful("Not a eligible channel");
            }
        }

        public async Task<bool> AddOrRemoveChannel(CachedTextChannel channel, DbService db)
        {
            var check = await db.IgnoreChannels.FindAsync(channel.Guild.Id.RawValue, channel.Id.RawValue);
            if (check != null)
            {
                var ch = ChannelEnable.GetOrAdd(channel.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
                ch.TryRemove(channel.Id.RawValue, out _);

                var result =
                    await db.IgnoreChannels.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id.RawValue && x.ChannelId == channel.Id.RawValue);
                db.IgnoreChannels.Remove(result);
                await db.SaveChangesAsync();
                return false;
            }
            else
            {
                var ch = ChannelEnable.GetOrAdd(channel.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
                ch.TryAdd(channel.Id.RawValue, true);

                var data = new IgnoreChannel
                {
                    GuildId = channel.Guild.Id.RawValue,
                    ChannelId = channel.Id.RawValue
                };
                db.IgnoreChannels.Update(data);
                await db.IgnoreChannels.AddAsync(data);
                await db.SaveChangesAsync();
                return true;
            }
        }

        private static async Task<bool> UpdateIgnoreAllStatus(DiscordCommandContext context)
        {
            using var scope = context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(context.Guild);
            return cfg.IgnoreAllChannels;
        }

        private static bool EligibleChannel(DiscordCommandContext context, bool ignoreAll = false)
        {
            // True = command passes
            // False = command fails
            var ch = ChannelEnable.GetOrAdd(context.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
            var ignore = ch.TryGetValue(context.Channel.Id.RawValue, out var status);
            if (!ignore) ignore = DoubleCheckChannel(context);
            return !ignoreAll ? !ignore : ignore;
        }

        private static bool DoubleCheckChannel(DiscordCommandContext context)
        {
            using var scope = context.ServiceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = db.IgnoreChannels.Find(context.Guild.Id.RawValue, context.Channel.Id.RawValue);
            if (check == null) return false;
            var ch = ChannelEnable.GetOrAdd(context.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
            ch.TryAdd(context.Channel.Id.RawValue, true);
            return true;
        }
    }
}