using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Hanekawa.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.AutoMessage
{
    public class AutoMessageService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colour;
        private readonly InternalLogService _log;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<string, Timer>> _timers = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Timer>>();

        public AutoMessageService(Hanekawa client, IServiceProvider provider, ColourService colour, InternalLogService log)
        {
            _client = client;
            _provider = provider;
            _colour = colour;
            _log = log;

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
                    try
                    {
                        await x.DisposeAsync();
                    }
                    catch 
                    { /* IGNORE */}
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
            var timer = CreateTimer(channel, firstPost, interval, message, user.Guild.Id.RawValue, name);
            if (timers.TryAdd(name, timer))
            {
                await AddToDatabase(user, channel, interval, name, message);
                return true;
            }
            await timer.DisposeAsync();
            return false;
        }

        public bool RemoveAutoMessage(ulong guildId, string name, DbService db)
        {
            if (!_timers.TryGetValue(guildId, out var timers)) return false;
            if (!timers.TryRemove(name, out var timer)) return false;
            timer?.Dispose();
            return RemoveFromDb(guildId, name, db);
        }

        public List<string> GetList(ulong guildId)
        {
            var timers = _timers.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
            var result = new List<string>();
            if (timers.IsEmpty) return null;
            foreach (var (name, timer) in timers)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Name: {name}\n" +
                              $"Interval: {timer}");
                sb.AppendLine();
                result.Add(sb.ToString());
            }
            return result;
        }

        public async Task<bool> EditMessageAsync(ulong guildId, string name, string newMessage, DbService db)
        {
            try
            {
                var timers = _timers.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
                if (!timers.TryGetValue(name, out var timer)) return false;
                var dbTimer = await db.AutoMessages.FindAsync(guildId, name);
                if (dbTimer == null)
                {
                    await timer.DisposeAsync();
                    return false;
                }

                dbTimer.Message = newMessage;
                var firstPost = new TimeSpan(0, (60 - DateTime.UtcNow.Minute), 0);
                var newTimer = CreateTimer(_client.GetGuild(guildId).GetTextChannel(dbTimer.ChannelId), firstPost,
                    dbTimer.Interval, newMessage, guildId, name);
                timers.AddOrUpdate(name, newTimer, (s, _) => newTimer);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Auto Message Service) Couldn't edit message for existing timer for {guildId}");
                return false;
            }
        }

        public async Task<bool> EditIntervalAsync(ulong guildId, string name, TimeSpan newInterval, DbService db)
        {
            var timers = _timers.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
            if (!timers.TryGetValue(name, out var timer)) return false;
            var dbTimer = await db.AutoMessages.FindAsync(guildId, name);
            if (dbTimer == null)
            {
                await timer.DisposeAsync();
                return false;
            }

            dbTimer.Interval = newInterval;
            var firstPost = new TimeSpan(0, (60 - DateTime.UtcNow.Minute), 0);
            var change = timer.Change(firstPost, newInterval);
            if (change) await db.SaveChangesAsync();
            return change;
        }

        private Timer CreateTimer(CachedTextChannel channel, TimeSpan firstPost, TimeSpan interval, string message,
            ulong guildId, string name) =>
            new Timer(async _ =>
            {
                var timers = _timers.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
                var guild = _client.GetGuild(guildId);
                if (guild == null)
                {
                    timers.TryRemove(name, out var toDispose);
                    if (toDispose != null) await toDispose.DisposeAsync();
                    RemoveFromDb(guildId, name);
                    return;
                }

                var txt = guild.GetTextChannel(channel.Id.RawValue);
                if (txt == null)
                {
                    timers.TryRemove(name, out var toDispose);
                    if (toDispose != null) await toDispose.DisposeAsync();
                    RemoveFromDb(guildId, name);
                    return;
                }

                await channel.SendMessageAsync(null, false,
                    new LocalEmbedBuilder().Create(MessageUtil.FormatMessage(message, null, txt.Guild), _colour.Get(guildId)).Build());
            }, null, new TimeSpan(0, (60 - DateTime.UtcNow.Minute), 0), interval);

        private async Task AddToDatabase(CachedMember user, CachedTextChannel channel, TimeSpan interval, string name, string message)
        {
            try
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
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Auto Message Service) Couldn't save auto message to database for {user.Guild.Id.RawValue}");
                throw;
            }
        }

        private bool RemoveFromDb(ulong guildId, string name, DbService db)
        {
            try
            {
                var toRemove = db.AutoMessages.Find(guildId, name);
                if (toRemove == null) return false;
                db.AutoMessages.Remove(toRemove);
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Auto Message Service) Couldn't remove {name} from {guildId}");
                return false;
            }
        }

        private bool RemoveFromDb(ulong guildId, string name)
        {
            try
            {
                using var scope = _provider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var toRemove = db.AutoMessages.Find(guildId, name);
                if (toRemove == null) return false;
                db.AutoMessages.Remove(toRemove);
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Auto Message Service) Couldn't remove {name} from {guildId}");
                return false;
            }
        }
    }
}