using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Preconditions
{
    public class RequiredChannel : CheckAttribute, INService
    {
        private static ConcurrentDictionary<Snowflake, bool> IgnoreAll { get; } = new();
        private static ConcurrentDictionary<Snowflake, bool> ChannelEnable { get; } = new();

        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("Wrong command context.");
            var roles = context.Author.GetRoles();
            if (Discord.Permissions.CalculatePermissions(context.Guild, context.Author, roles.Values).Has(Permission.ManageGuild))
                return CheckResult.Successful;

            var ignoreAll = IgnoreAll.TryGetValue(context.Guild.Id, out var status);
            if (!ignoreAll) status = await UpdateIgnoreAllStatus(context);

            var pass = status ? EligibleChannel(context, true) : EligibleChannel(context);

            return pass switch
            {
                true => CheckResult.Successful,
                false => CheckResult.Failed("Cannot execute this command in this channel.")
            };
        }

        public async ValueTask<bool> AddOrRemoveChannel(CachedTextChannel channel, DbService db)
        {
            var check = await db.IgnoreChannels.FindAsync(channel.GuildId.RawValue, channel.Id.RawValue);
            if (check != null)
            {
                ChannelEnable.TryRemove(channel.Id, out _);
                var result =
                    await db.IgnoreChannels.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.GuildId.RawValue && x.ChannelId == channel.Id.RawValue);
                db.IgnoreChannels.Remove(result);
                await db.SaveChangesAsync();
                return false;
            }
            ChannelEnable.GetOrAdd(channel.Id, true);
            var data = new IgnoreChannel 
            {
                GuildId = channel.GuildId.RawValue, 
                ChannelId = channel.Id.RawValue
                
            }; 
            await db.IgnoreChannels.AddAsync(data); 
            await db.SaveChangesAsync(); 
            return true; 
        }

        private static async ValueTask<bool> UpdateIgnoreAllStatus(HanekawaCommandContext context)
        {
            using var scope = context.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(context.Guild);
            return cfg.IgnoreAllChannels;
        }

        private static bool EligibleChannel(HanekawaCommandContext context, bool ignoreAll = false)
        {
            // True = command passes
            // False = command fails
            var ignore = ChannelEnable.TryGetValue(context.Channel.Id.RawValue, out _);
            if (!ignore) ignore = DoubleCheckChannel(context);
            return !ignoreAll ? !ignore : ignore;
        }

        private static bool DoubleCheckChannel(HanekawaCommandContext context)
        {
            using var scope = context.Services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = db.IgnoreChannels.Find(context.Guild.Id.RawValue, context.Channel.Id.RawValue);
            if (check == null) return false;
            ChannelEnable.TryAdd(context.Channel.Id.RawValue, true);
            return true;
        }
    }
}
