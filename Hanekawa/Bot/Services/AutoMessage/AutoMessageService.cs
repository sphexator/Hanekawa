using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.AutoMessage
{
    public class AutoMessageService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colour;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<string, Timer>> _timers = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Timer>>();

        public AutoMessageService(Hanekawa client, IServiceProvider provider, ColourService colour)
        {
            _client = client;
            _provider = provider;
            _colour = colour;

            _client.LeftGuild += ClearMessages;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.AutoMessages.ToList())
            {
                var timers = _timers.GetOrAdd(x.GuildId, new ConcurrentDictionary<string, Timer>());
                timers.TryAdd(x.Name, new Timer(async _ =>
                {
                    var guild = _client.GetGuild(x.GuildId);
                    if (guild == null)
                    {
                        timers.TryRemove(x.Name, out var toDispose);
                        if (toDispose != null) await toDispose.DisposeAsync();
                        RemoveFromDb(x.GuildId, x.Name);
                        return;
                    }
                    var channel = guild.GetTextChannel(x.ChannelId);
                    if (channel == null)
                    {
                        timers.TryRemove(x.Name, out var toDispose);
                        if (toDispose != null) await toDispose.DisposeAsync();
                        RemoveFromDb(x.GuildId, x.Name);
                        return;
                    }
                    await channel.SendMessageAsync(null, false,
                        new LocalEmbedBuilder().Create(x.Message, _colour.Get(x.GuildId)).Build());
                }, null, new TimeSpan(0, (60 - DateTime.UtcNow.Minute), 0), x.Interval));
            }
        }

        private Task ClearMessages(LeftGuildEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!_timers.TryGetValue(e.Guild.Id.RawValue, out var timers)) return;
                foreach (var x in timers.Values)
                {
                    await x.DisposeAsync();
                }

                _timers.TryRemove(e.Guild.Id.RawValue, out _);
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var toRemove = await db.AutoMessages.Where(x => x.GuildId == e.Guild.Id.RawValue).ToListAsync();
                db.AutoMessages.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        public async Task<bool> AddAutoMessage(CachedMember user, CachedTextChannel channel, TimeSpan interval, string name, string message)
        {
            var timers = _timers.GetOrAdd(user.Guild.Id.RawValue, new ConcurrentDictionary<string, Timer>());
            if (timers.Count >= 3) return false;
            var firstPost = new TimeSpan(0, (60 - DateTime.UtcNow.Minute), 0);
            var timer = new Timer(async _ =>
            {
                await channel.SendMessageAsync(null, false,
                    new LocalEmbedBuilder().Create(message, _colour.Get(user.Guild.Id.RawValue)).Build());
            }, null, firstPost, interval);
            if (timers.TryAdd(name, timer))
            {
                await AddToDatabase(user, channel, interval, name, message);
                return true;
            }
            await timer.DisposeAsync();
            return false;
        }

        public void RemoveAutoMessage(ulong guildId, string name, DbService db)
        {
            if (!_timers.TryGetValue(guildId, out var timers)) return;
            timers.TryRemove(name, out var timer);
            timer?.Dispose();
            RemoveFromDb(guildId, name, db);
        }

        private async Task AddToDatabase(CachedMember user, CachedTextChannel channel, TimeSpan interval, string name, string message)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            await db.AutoMessages.AddAsync(new Database.Tables.AutoMessage.AutoMessage
            {
                GuildId = user.Guild.Id.RawValue,
                Creator = user.Id.RawValue,
                ChannelId = channel.Id.RawValue,
                Name = name,
                Message = message,
                Interval = interval
            });
            await db.SaveChangesAsync();
        }

        private void RemoveFromDb(ulong guildId, string name, DbService db)
        {
            var toRemove = db.AutoMessages.Find(guildId, name);
            db.AutoMessages.Remove(toRemove);
            db.SaveChanges();
        }

        private void RemoveFromDb(ulong guildId, string name)
        {
            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var toRemove = db.AutoMessages.Find(guildId, name);
            db.AutoMessages.Remove(toRemove);
            db.SaveChanges();
        }
    }
}