using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Hosting;
using Hanekawa.Bot.Commands;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.Cache
{
    public class CacheService : DiscordClientService
    {
        // Guild Settings
        private readonly ConcurrentDictionary<Snowflake, HashSet<IPrefix>> _guildPrefix = new();
        private readonly ConcurrentDictionary<Snowflake, Color> _guildEmbedColors = new();
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<EmoteType, IEmoji>> _emote = new();
        // Cooldown
        private readonly MemoryCache _globalCooldown = new(new MemoryCacheOptions());
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<CooldownType, MemoryCache>> _cooldown = new();
        // Game
        private readonly ConcurrentDictionary<Snowflake, ShipGameType> _shipGames = new();
        // Level
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<ExpSource, double>> _experienceMultipliers = new();
        private readonly ConcurrentDictionary<Snowflake, Timer> _expEvents = new();
        private readonly HashSet<Snowflake> _experienceReduction = new();
        // Administration
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Timer>> _muteTimers = new();
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _banCache = new();
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<string, Tuple<Snowflake?, int>>> _guildInvites = new();
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<string, Timer>> _autoMessages = new();
        // Quotes
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _quoteCache = new();
        // Drops
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _activeDrops = new();
        private readonly ConcurrentDictionary<Snowflake, HashSet<Snowflake>> _dropChannels = new();
        // Command whitelists
        private static ConcurrentDictionary<Snowflake, bool> IgnoreAll { get; } = new();
        private static ConcurrentDictionary<Snowflake, bool> ChannelEnable { get; } = new();

        private readonly IServiceProvider _provider;
        
        public CacheService(ILogger<CacheService> logger, Hanekawa client, IServiceProvider provider) : base(logger, client) => _provider = provider;

        // ----- Guild Settings -----
        public Color GetColor(Snowflake guildSnowflake) =>
            _guildEmbedColors.TryGetValue(guildSnowflake, out var color) 
                ? color 
                : HanaBaseColor.Default();
        public void AddOrUpdateColor(Snowflake guildId, Color color) => _guildEmbedColors.GetOrAdd(guildId, color);

        public HashSet<IPrefix> GetPrefix(Snowflake guildId) => 
            (_guildPrefix.TryGetValue(guildId, out var prefix) 
                ? prefix 
                : null);
        public bool AddOrUpdatePrefix(Snowflake guildId, IPrefix prefix) =>
            _guildPrefix.GetOrAdd(guildId, new HashSet<IPrefix>()).Add(prefix);
        
        public bool TryGetEmote(EmoteType type, Snowflake guildId, out IEmoji emote)
            => _emote.GetOrAdd(guildId, new ConcurrentDictionary<EmoteType, IEmoji>()).TryGetValue(type, out emote!);

        public void AddOrUpdateEmote(EmoteType type, Snowflake guildId, IEmoji emote)
        {
            _emote.GetOrAdd(guildId, new ConcurrentDictionary<EmoteType, IEmoji>())
                .AddOrUpdate(type, emote, (_, _) => emote);
        }

        // ----- Cooldown -----
        public bool TryGetCooldown(Snowflake guildId, Snowflake userId, CooldownType type) =>
            _cooldown.GetOrAdd(guildId, new ConcurrentDictionary<CooldownType, MemoryCache>())
                .GetOrAdd(type, new MemoryCache(new MemoryCacheOptions()))
                .TryGetValue(userId, out _);

        public void AddCooldown(Snowflake guildId, Snowflake userId, CooldownType type) =>
            _cooldown.GetOrAdd(guildId, new ConcurrentDictionary<CooldownType, MemoryCache>())
                .GetOrAdd(type, new MemoryCache(new MemoryCacheOptions()))
                .Set(userId, 0, TimeSpan.FromMinutes(1));

        public bool TryGetGlobalCooldown(Snowflake userId) => _globalCooldown.TryGetValue(userId, out _);
        public void AddGlobalCooldown(Snowflake userId) => _globalCooldown.Set(userId, 0, TimeSpan.FromMinutes(1));
        
        // ----- Game -----
        public bool TryGetShipGame(Snowflake channelId) => _shipGames.TryGetValue(channelId, out _);
        public bool AddGame(Snowflake channelId, ShipGameType type) => _shipGames.TryAdd(channelId, type);
        public bool RemoveGame(Snowflake channelId) => _shipGames.TryRemove(channelId, out _);

        // ----- Level -----
        public void AdjustExpMultiplier(ExpSource type, Snowflake guildId, double multiplier)
            => _experienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<ExpSource, double>())
                .AddOrUpdate(type, multiplier, (_, _) => multiplier);

        public bool TryGetMultiplier(ExpSource type, Snowflake guildId, out double multiplier)
            => _experienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<ExpSource, double>())
                .TryGetValue(type, out multiplier);

        public bool IsExpReduced(Snowflake channelId) => _experienceReduction.TryGetValue(channelId, out _);
        
        public bool TryAddExpChannelReduction(Snowflake channelId)
        {
            if (_experienceReduction.TryGetValue(channelId, out _)) return false;
            _experienceReduction.Add(channelId);
            return true;
        }
        public bool TryRemoveExpReduction(Snowflake channelId)
        {
            if (!_experienceReduction.TryGetValue(channelId, out _)) return false;
            _experienceReduction.Remove(channelId);
            return true;
        }

        public bool TryAddExpEvent(Snowflake guildId, Timer timer)
        {
            if (!_expEvents.TryGetValue(guildId, out var value)) return _expEvents.TryAdd(guildId, timer);
            try { value.Dispose(); }
            catch { /* IGNORE */ }
            _expEvents.TryRemove(guildId, out _);
            return _expEvents.TryAdd(guildId, timer);
        }

        public bool TryRemoveExpEvent(Snowflake guildId)
        {
            if (!_expEvents.TryRemove(guildId, out var timer)) return false;
            try { timer.Dispose(); }
            catch { /* IGNORE */ }
            return true;
        }

        public bool TryGetExpEvent(Snowflake guildId) => _expEvents.TryGetValue(guildId, out _);

        // ----- Administration -----
        public bool TryGetMute(Snowflake guildId, Snowflake userId)
        {
            var mutes = _muteTimers.GetOrAdd(guildId, new ConcurrentDictionary<Snowflake, Timer>());
            return mutes.TryGetValue(userId, out _);
        }
        
        public void AddOrUpdateMuteTimer(Snowflake guildId, Snowflake userId, Timer timer)
        {
            var mutes = _muteTimers.GetOrAdd(guildId, new ConcurrentDictionary<Snowflake, Timer>());
            mutes.AddOrUpdate(userId, timer, (_, old) =>
            {
                old.Dispose();
                return timer;
            });
            _muteTimers.AddOrUpdate(guildId, mutes, (_, _) => mutes);
        }

        public void RemoveMuteTimer(Snowflake guildId, Snowflake userId)
        {
            var mutes = _muteTimers.GetOrAdd(guildId, new ConcurrentDictionary<Snowflake, Timer>());
            if (!mutes.TryRemove(userId, out var timer)) return;
            timer.Dispose();
        }
        
        public bool TryGetAutoMessage(Snowflake guildId, string name)
        {
            var autoMessages = _autoMessages.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
            return autoMessages.TryGetValue(name, out _);
        }
        
        public void AddOrUpdateAutoMessageTimer(Snowflake guildId, string name, Timer timer)
        {
            if(!_autoMessages.TryGetValue(guildId, out var autoMessages))
            {
                _autoMessages.TryAdd(guildId, new ConcurrentDictionary<string, Timer>());
                _autoMessages.TryGetValue(guildId, out autoMessages);
            }
            
            if (autoMessages != null && !autoMessages.TryAdd(name, timer))
            {
                if(autoMessages.TryRemove(name, out var toDispose)) toDispose.Dispose();
                autoMessages.TryAdd(name, timer);
            }

            _autoMessages.TryUpdate(guildId, autoMessages, null);
        }

        public void RemoveAutoMessageTimer(Snowflake guildId, string name)
        {
            var mutes = _autoMessages.GetOrAdd(guildId, new ConcurrentDictionary<string, Timer>());
            if (!mutes.TryRemove(name, out var timer)) return;
            timer.Dispose();
        }
        
        public Snowflake? TryGetBanCache(Snowflake guildId, Snowflake userId)
        {
            if (!_banCache.TryGetValue(guildId, out var banCache)) return null;
            banCache.TryGetValue(userId, out var staff);
            return staff is Snowflake result ? result : null;
        }
        
        public void AddBanCache(Snowflake guildId, Snowflake staffId, Snowflake userId)
        {
            var banCache = _banCache.GetOrAdd(guildId, new MemoryCache(new MemoryCacheOptions()));
            if(banCache.TryGetValue(userId, out _)) return;
            banCache.Set(userId, staffId, TimeSpan.FromMinutes(30));
        }

        public bool TryGetInvite(Snowflake guildId, out ConcurrentDictionary<string, Tuple<Snowflake?, int>> inviteTuple)
        {
            inviteTuple = null;
            return _guildInvites.TryGetValue(guildId, out inviteTuple);
        }

        public void AddInvite(Snowflake guildId, IInvite code)
        {
            var channelCodes =
                _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            var toAdd = new Tuple<Snowflake?, int>(code.Inviter.Id, code.Metadata.Uses);
            
            channelCodes.AddOrUpdate(code.Code, new Tuple<Snowflake?, int>(code.Inviter.Id, code.Metadata.Uses),
                (_, _) => toAdd);
            _guildInvites.AddOrUpdate(guildId, channelCodes, (_, _) => channelCodes);
        }

        public void RemoveInvite(Snowflake guildId, string code)
        {
            var channelCodes = _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            channelCodes.TryRemove(code, out _);
            _guildInvites.AddOrUpdate(guildId, channelCodes, (_, _) => channelCodes);
        }

        private void UpdateInvite(Snowflake guildId, string code, Snowflake? invitee, int inviteUses)
        {
            _guildInvites.AddOrUpdate(guildId,
                _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>()),
                (_, invites) =>
                {
                    invites.AddOrUpdate(code, new Tuple<Snowflake?, int>(invitee, inviteUses),
                        (_, _) => new Tuple<Snowflake?, int>(invitee, inviteUses));
                    return invites;
                });
        }
        
        public void UpdateInvites(Snowflake guildId, IEnumerable<IInvite> restInvites)
        {
            foreach (var x in restInvites) 
                UpdateInvite(guildId, x.Code, x.Inviter.Id, x.Metadata.Uses);
        }
        
        // ----- DROP -----
        public bool GetDrop(Snowflake channelId, Snowflake messageId, out DropType type)
        {
            var activeDropCache = _activeDrops.GetOrAdd(channelId, new MemoryCache(new MemoryCacheOptions()));
            return activeDropCache.TryGetValue(messageId, out type);
        }
        
        public void AddDrop(Snowflake channelId, Snowflake messageId, DropType type)
        {
            var activeDropCache = _activeDrops.GetOrAdd(channelId, new MemoryCache(new MemoryCacheOptions()));
            activeDropCache.Set(messageId, type, TimeSpan.FromHours(1));
        }

        public void RemoveDrop(Snowflake channelId, Snowflake messageId)
        {
            var activeDropCache = _activeDrops.GetOrAdd(channelId, new MemoryCache(new MemoryCacheOptions()));
            activeDropCache.Remove(messageId);
        }

        public void AddDropChannel(Snowflake guildId, Snowflake channelId)
        {
            var dropChannel = _dropChannels.GetOrAdd(guildId, new HashSet<Snowflake>());
            dropChannel.Add(channelId);
        }

        public void RemoveDropChannel(Snowflake guildId, Snowflake channelId)
        {
            var dropChannel = _dropChannels.GetOrAdd(guildId, new HashSet<Snowflake>());
            dropChannel.Remove(channelId);
        }

        public bool TryGetDropChannel(Snowflake guildId, Snowflake channelId)
        {
            var dropChannel = _dropChannels.GetOrAdd(guildId, new HashSet<Snowflake>());
            return dropChannel.TryGetValue(channelId, out _);
        }
        
        // Command blacklist / whitelists
        public bool TryGetIgnoreChannel(Snowflake guildId, out bool status) =>
            IgnoreAll.TryGetValue(guildId, out status);

        public void UpdateIgnoreAllChannel(Snowflake guildId, bool status)
        {
            if (IgnoreAll.TryAdd(guildId, status)) return;
            IgnoreAll.TryGetValue(guildId, out var oldValue);
            IgnoreAll.TryUpdate(guildId, status, oldValue);
        }
        
        public async ValueTask<bool> AddOrRemoveChannel(ITextChannel channel, DbService db)
        {
            var check = await db.IgnoreChannels.FindAsync(channel.GuildId, channel.Id);
            if (check != null)
            {
                ChannelEnable.TryRemove(channel.Id, out _);
                var result =
                    await db.IgnoreChannels.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.GuildId && x.ChannelId == channel.Id);
                db.IgnoreChannels.Remove(result);
                await db.SaveChangesAsync();
                return false;
            }
            ChannelEnable.GetOrAdd(channel.Id, true);
            var data = new IgnoreChannel 
            {
                GuildId = channel.GuildId, 
                ChannelId = channel.Id
                
            }; 
            await db.IgnoreChannels.AddAsync(data); 
            await db.SaveChangesAsync(); 
            return true; 
        }

        public async ValueTask<bool> UpdateIgnoreAllStatus(HanekawaCommandContext context)
        {
            using var scope = context.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(context.Guild);
            UpdateIgnoreAllChannel(cfg.GuildId, cfg.IgnoreAllChannels);
            return cfg.IgnoreAllChannels;
        }

        public bool EligibleChannel(HanekawaCommandContext context, bool ignoreAll = false)
        {
            // True = command passes
            // False = command fails
            var ignore = ChannelEnable.TryGetValue(context.Channel.Id, out _);
            if (!ignore) ignore = DoubleCheckChannel(context);
            return !ignoreAll ? !ignore : ignore;
        }

        public bool DoubleCheckChannel(HanekawaCommandContext context)
        {
            using var scope = context.Services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = db.IgnoreChannels.Find(context.Guild.Id, context.Channel.Id);
            if (check == null) return false;
            ChannelEnable.TryAdd(context.Channel.Id, true);
            return true;
        }

        protected override ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            _cooldown.TryRemove(e.GuildId, out _);
            _guildInvites.TryRemove(e.GuildId, out _);
            _muteTimers.TryRemove(e.GuildId, out _);
            _experienceMultipliers.TryRemove(e.GuildId, out _);
            _guildPrefix.TryRemove(e.GuildId, out _);
            _banCache.TryRemove(e.GuildId, out _);
            _quoteCache.TryRemove(e.GuildId, out _);
            _guildEmbedColors.TryRemove(e.GuildId, out _);
            _emote.TryRemove(e.GuildId, out _);
            return ValueTask.CompletedTask;
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.LevelConfigs)
            {
                AdjustExpMultiplier(ExpSource.Text, x.GuildId, x.TextExpMultiplier);
                AdjustExpMultiplier(ExpSource.Voice, x.GuildId, x.VoiceExpMultiplier);
                AdjustExpMultiplier(ExpSource.Other, x.GuildId, 1);
            }

            foreach (var x in db.LevelExpReductions)
                TryAddExpChannelReduction(x.ChannelId);

            foreach (var x in db.GuildConfigs)
            {
                AddOrUpdatePrefix(x.GuildId, new StringPrefix(x.Prefix));
                AddOrUpdateColor(x.GuildId, new Color(x.EmbedColor));
            }
        }
    }
}