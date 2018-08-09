﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredChannel : RequireContextAttribute
    {
        private ConcurrentDictionary<ulong, bool> IgnoreAll { get; set; }
            = new ConcurrentDictionary<ulong, bool>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> ChannelEnable { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        public RequiredChannel() : base(ContextType.Guild) { }
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            var ignrAll = IgnoreAll.TryGetValue(context.Guild.Id, out var status);
            if (!ignrAll) status = await UpdateIgnoreAllStatus(context);

            bool pass;

            pass = status ? EligibleChannel(context, true) : EligibleChannel(context);

            switch (pass)
            {
                case true:
                    return PreconditionResult.FromSuccess();
                case false:
                    return PreconditionResult.FromError("Not eligible channel");
                default:
                    return PreconditionResult.FromError("Not eligible channel");
            }
        }

        private static async Task<bool> UpdateIgnoreAllStatus(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(context.Guild as SocketGuild);
                return cfg.IgnoreAllChannels;
            }
        }

        private bool EligibleChannel(ICommandContext context, bool ignoreAll = false)
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

        private bool DoubleCheckChannel(ICommandContext context)
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

        public async Task<bool> AddChannel(ITextChannel channel)
        {
            using (var db = new DbService())
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
        }

        public async Task<bool> RemoveChannel(ITextChannel channel)
        {
            using (var db = new DbService())
            {
                var check = await db.IgnoreChannels.FindAsync(channel.GuildId, channel.Id);
                if (check == null) return false;

                var ch = ChannelEnable.GetOrAdd(channel.GuildId, new ConcurrentDictionary<ulong, bool>());
                ch.TryRemove(channel.Id, out var value);

                var result = await db.IgnoreChannels.FirstOrDefaultAsync(x => x.GuildId == channel.GuildId && x.ChannelId == channel.Id);
                db.IgnoreChannels.Remove(result);
                await db.SaveChangesAsync();
                return true;
            }
        }
    }
}