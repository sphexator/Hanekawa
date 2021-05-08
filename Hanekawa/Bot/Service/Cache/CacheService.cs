using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hanekawa.Bot.Service.Cache
{
    public class CacheService : INService
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
        private readonly HashSet<Snowflake> _experienceReduction = new();
        // Administration
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Timer>> _muteTimers = new();
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _banCache = new();
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<string, Tuple<Snowflake?, int>>> _guildInvites = new();
        // Board
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _board = new();
        // Quotes
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _quoteCache = new();
        // Drops
        private readonly ConcurrentDictionary<Snowflake, MemoryCache> _activeDrops = new();
        private readonly ConcurrentDictionary<Snowflake, HashSet<Snowflake>> _dropChannels = new();
        
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
            var channelCodes = _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            var toAdd = new Tuple<Snowflake?, int>(code.Inviter.Value.Id, code.Metadata.Uses);
            var codes = channelCodes.AddOrUpdate(code.Code,
                new Tuple<Snowflake?, int>(code.Inviter.Value.Id, code.Metadata.Uses), (_, _) => toAdd);
            _guildInvites.AddOrUpdate(guildId, channelCodes, (_, _) => channelCodes);
        }

        public void RemoveInvite(Snowflake guildId, string code)
        {
            var channelCodes = _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            channelCodes.TryRemove(code, out _);
            _guildInvites.AddOrUpdate(guildId, channelCodes, (_, _) => channelCodes);
        }

        public void UpdateInvite(Snowflake guildId, string code, Snowflake? invitee, int inviteUses)
        {
            _guildInvites.AddOrUpdate(guildId,
                _guildInvites.GetOrAdd(guildId, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>()),
                (_, invites) =>
                {
                    invites.AddOrUpdate(code, new Tuple<Snowflake?, int>(invitee, inviteUses),
                        (s, tuple) => new Tuple<Snowflake?, int>(invitee, inviteUses));
                    return invites;
                });
        }
        
        public void UpdateInvites(Snowflake guildId, IEnumerable<IInvite> restInvites)
        {
            foreach (var x in restInvites) 
                UpdateInvite(guildId, x.Code, x.Inviter.Value.Id, x.Metadata.Uses);
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

        public void Dispose(Snowflake guildId)
        {
            _cooldown.TryRemove(guildId, out _);
            _guildInvites.TryRemove(guildId, out _);
            _muteTimers.TryRemove(guildId, out _);
            _experienceMultipliers.TryRemove(guildId, out _);
            _guildPrefix.TryRemove(guildId, out _);
            _banCache.TryRemove(guildId, out _);
            _quoteCache.TryRemove(guildId, out _);
            _guildEmbedColors.TryRemove(guildId, out _);
            _emote.TryRemove(guildId, out _);
        }
    }
}