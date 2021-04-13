using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using static Disqord.LocalCustomEmoji;

namespace Hanekawa.Bot.Service.Drop
{
    public class DropService : INService
    {
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly Experience _exp;
        private readonly Random _random;
        
        public DropService(Hanekawa bot, CacheService cache, IServiceProvider provider, Experience exp, Random random)
        {
            _bot = bot;
            _cache = cache;
            _provider = provider;
            _exp = exp;
            _random = random;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Message is not IUserMessage msg) return;
            var cooldown =
                _cache.Cooldown.GetOrAdd(e.GuildId.Value, new ConcurrentDictionary<CooldownType, MemoryCache>());

            var channelCd = cooldown.GetOrAdd(CooldownType.Drop, new MemoryCache(new MemoryCacheOptions()));
            if (channelCd.TryGetValue(e.ChannelId, out _)) return;
            
            var rand = _random.Next(0, 10000);
            if (rand < 200)
                try
                {
                    await SpawnAsync(e.GuildId.Value, e.Channel, msg, DropType.Regular);
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception,
                        $"(Drop Service) Error in {e.GuildId.Value.RawValue} for drop create - {exception.Message}");
                }
        }
        
        public async Task ReactionReceived(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            var cache = _cache.Drops.GetOrAdd(e.GuildId.Value, new MemoryCache(new MemoryCacheOptions()));
            if (cache.TryGetValue(e.MessageId, out var dropObject)) return;
            if (dropObject is not DropType dropType) return;
            cache.Remove(e.MessageId);
            await ClaimAsync(e.Message, e.Member, dropType);
        }
        
        private async Task SpawnAsync(Snowflake guildId, IMessageChannel channel, IMessage msg, DropType type)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var guild = _bot.GetGuild(guildId);
            var claim = await GetClaimEmoteAsync(guild, db);
            var triggerMsg = await channel.SendMessageAsync(new LocalMessageBuilder
            {
                Content = $"A {type.ToString().ToLower()} drop event has been triggered \nClick the {claim} reaction on this message to claim it!",
                Attachments = null,
                Embed = null,
                Mentions = LocalMentionsBuilder.None,
                Reference = new LocalReferenceBuilder
                    {GuildId = guild.Id, ChannelId = msg.ChannelId, MessageId = msg.Id, FailOnInvalid = false},
                IsTextToSpeech = false
            }.Build());
            var emotes = GetEmotes(guild);
            foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                await ApplyReactionAsync(triggerMsg, x, claim, type);

            _logger.Log(LogLevel.Info, $"(Drop Service) Drop event created in {guildId.RawValue}");
        }

        private async Task ClaimAsync(IUserMessage msg, IMember user, DropType type)
        {
            try { await msg.DeleteAsync(); }
            catch { /* Ignore */}
            
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var rand = type == DropType.Regular 
                ? _random.Next(15, 150) 
                : _random.Next(150, 250);
            
            var userData = await db.GetOrCreateUserData(user);
            var cfg = await db.GetOrCreateCurrencyConfigAsync(user.GuildId.RawValue);
            var exp = await _exp.AddExpAsync(user, userData, rand, rand, db, ExpSource.Other);
            var trgMsg = await _bot.SendMessageAsync(msg.ChannelId, new LocalMessageBuilder 
            {
                Content = $"Rewarded {user.Mention} with {exp} experience & {cfg.ToCurrencyFormat(rand)} {cfg.CurrencyName}!",
                Attachments = null,
                Embed = null,
                Mentions = LocalMentionsBuilder.None,
                IsTextToSpeech = false,
                Reference = null
            }.Build());
            
            try
            {
                await Task.Delay(5000);
                await trgMsg.DeleteAsync();
            }
            catch { /* Ignore */}
        }

        private async Task<IEmoji> GetClaimEmoteAsync(IGuild guild, DbService db)
        {
            var cache = _cache.Emote.GetOrAdd(guild.Id, new ConcurrentDictionary<EmoteType, IEmoji>());
            if (cache.TryGetValue(EmoteType.Drop, out var emote)) return emote;
            var cfg = await db.GetOrCreateDropConfigAsync(guild);
            if (TryParse(cfg.Emote, out var sEmote))
            {
                cache.AddOrUpdate(EmoteType.Drop, sEmote, (_, _) => sEmote);
                return sEmote;
            }

            var defaultEmote = new LocalCustomEmoji(456747197854384158);
            cache.AddOrUpdate(EmoteType.Drop, defaultEmote, (_, _) => defaultEmote); 
            return defaultEmote;
        }

        private List<IGuildEmoji> GetEmotes(IGuild guild) =>
            guild.Emojis.Count >= 4 
                ? guild.Emojis.Values.ToList() 
                : _bot.GetGuild(431617676859932704).Emojis.Values.ToList();

        private async Task ApplyReactionAsync(IMessage message, IGuildEmoji emote, IEmoji claim, DropType type)
        {
            if (emote.Name == claim.Name)
                _cache.Drops.GetOrAdd(emote.GuildId, new MemoryCache(new MemoryCacheOptions()))
                    .Set(message.Id, type, TimeSpan.FromHours(1));
            await message.AddReactionAsync(emote);
        }
    }
}