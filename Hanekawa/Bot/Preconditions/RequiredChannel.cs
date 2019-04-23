using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Preconditions
{
    public class RequiredChannel : RequireContextAttribute, INService
    {
        public RequiredChannel() : base(ContextType.Guild)
        {
        }

        private ConcurrentDictionary<ulong, bool> IgnoreAll { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ChannelEnable { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user && user.GuildPermissions.ManageGuild)
                return PreconditionResult.FromSuccess();

            var ignoreAll = IgnoreAll.TryGetValue(context.Guild.Id, out var status);
            if (!ignoreAll) status = await UpdateIgnoreAllStatus(context, services);

            var pass = status ? EligibleChannel(context, services, true) : EligibleChannel(context, services);

            switch (pass)
            {
                case true:
                    return PreconditionResult.FromSuccess();
                case false:
                    return PreconditionResult.FromError("Not a eligible channel");
                default:
                    return PreconditionResult.FromError("Not a eligible channel");
            }
        }

        private async Task<bool> UpdateIgnoreAllStatus(ICommandContext context, IServiceProvider servcies)
        {
            var db = servcies.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(context.Guild as SocketGuild);
            return cfg.IgnoreAllChannels;
        }

        private bool EligibleChannel(ICommandContext context, IServiceProvider service, bool ignoreAll = false)
        {
            // True = command passes
            // False = command fails
            var ch = ChannelEnable.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            var ignore = ch.TryGetValue(context.Channel.Id, out var status);
            if (!ignore) ignore = DoubleCheckChannel(context, service);
            if (!ignoreAll) // If its only ignoring specific channels in the dictionary
                return !ignore;
            return ignore;
        }

        private bool DoubleCheckChannel(ICommandContext context, IServiceProvider services)
        {
            var db = services.GetRequiredService<DbService>();
            var check = db.IgnoreChannels.Find(context.Guild.Id, context.Channel.Id);
            if (check == null) return false;
            var ch = ChannelEnable.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            ch.TryAdd(context.Channel.Id, true);
            return true;
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
            ch.TryRemove(channel.Id, out var value);

            var result =
                await db.IgnoreChannels.FirstOrDefaultAsync(x =>
                    x.GuildId == channel.GuildId && x.ChannelId == channel.Id);
            db.IgnoreChannels.Remove(result);
            await db.SaveChangesAsync();
            return true;
        }
    }
}