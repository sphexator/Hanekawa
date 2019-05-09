using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    public class RequiredChannel : HanekawaAttribute, INService
    {
        public RequiredChannel() {  }

        private ConcurrentDictionary<ulong, bool> IgnoreAll { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ChannelEnable { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        public override async ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            if (context.User is SocketGuildUser user && user.GuildPermissions.ManageGuild)
                return CheckResult.Successful;

            var ignoreAll = IgnoreAll.TryGetValue(context.Guild.Id, out var status);
            if (!ignoreAll) status = await UpdateIgnoreAllStatus(context);

            var pass = status ? EligibleChannel(context, true) : EligibleChannel(context);

            switch (pass)
            {
                case true:
                    return CheckResult.Successful;
                case false:
                    return CheckResult.Unsuccessful("Not a eligible channel");
                default:
                    return CheckResult.Unsuccessful("Not a eligible channel");
            }
        }

        private async Task<bool> UpdateIgnoreAllStatus(HanekawaContext context)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(context.Guild);
                return cfg.IgnoreAllChannels;
            }
        }

        private bool EligibleChannel(HanekawaContext context, bool ignoreAll = false)
        {
            // True = command passes
            // False = command fails
            var ch = ChannelEnable.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var ignore = ch.TryGetValue(context.Channel.Id, out var status);
            if (!ignore) ignore = DoubleCheckChannel(context);
            if (!ignoreAll) // If its only ignoring specific channels in the dictionary
                return !ignore;
            return ignore;
        }

        private bool DoubleCheckChannel(HanekawaContext context)
        {
            using (var db = new DbService())
            {
                var check = db.IgnoreChannels.Find(context.Guild.Id, context.Channel.Id);
                if (check == null) return false;
                var ch = ChannelEnable.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
                ch.TryAdd(context.Channel.Id, true);
                return true;
            }
        }

        public async Task<bool> AddChannel(ITextChannel channel, DbService db)
        {
            var check = await db.IgnoreChannels.FindAsync(channel.GuildId, channel.Id);
            if (check != null) return false;

            var ch = ChannelEnable.GetOrAdd(channel.GuildId, new ConcurrentDictionary<ulong, bool>());
            ch.TryAdd(channel.Id, true);

            var data = new IgnoreChannel
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id
            };
            await db.IgnoreChannels.AddAsync(data);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveChannel(ITextChannel channel, DbService db)
        {
            var check = await db.IgnoreChannels.FindAsync(channel.GuildId, channel.Id);
            if (check == null) return false;

            var ch = ChannelEnable.GetOrAdd(channel.GuildId, new ConcurrentDictionary<ulong, bool>());
            ch.TryRemove(channel.Id, out _);

            var result =
                await db.IgnoreChannels.FirstOrDefaultAsync(x =>
                    x.GuildId == channel.GuildId && x.ChannelId == channel.Id);
            db.IgnoreChannels.Remove(result);
            await db.SaveChangesAsync();
            return true;
        }
    }
}