using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Bot.Service.Achievements;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using static Disqord.LocalCustomEmoji;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.Bot.Service.Drop
{
    public class DropService : DiscordClientService
    {
        private readonly AchievementService _achievement;
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;
        private readonly ExpService _exp;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;
        private readonly Random _random;

        public DropService(Hanekawa bot, CacheService cache, IServiceProvider provider, ExpService exp, Random random,
            AchievementService achievement, ILogger<DropService> logger) : base(logger, bot)
        {
            _bot = bot;
            _cache = cache;
            _provider = provider;
            _exp = exp;
            _random = random;
            _achievement = achievement;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Message is not IUserMessage msg) return;
            if (!_cache.TryGetDropChannel(e.GuildId.Value, e.ChannelId)) return;
            if (_cache.TryGetCooldown(e.GuildId.Value, e.Member.Id, CooldownType.Drop)) return;
            var rand = _random.Next(0, 10000);
            if (rand < 200)
                try
                {
                    await SpawnAsync(e.GuildId.Value, e.Channel, msg, DropType.Regular);
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception,
                        $"Error in {e.GuildId.Value} for drop create - {exception.Message}");
                }
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (!_cache.GetDrop(e.ChannelId, e.MessageId, out var type)) return;
            _cache.RemoveDrop(e.ChannelId, e.MessageId);
            await ClaimAsync(e.Message, e.Member, type);
        }

        public async Task SpawnAsync(Snowflake guildId, IMessageChannel channel, IMessage msg, DropType type)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var guild = _bot.GetGuild(guildId);
            var claim = await GetClaimEmoteAsync(guild, db);
            var triggerMsg = await channel.SendMessageAsync(new LocalMessage
            {
                Content =
                    $"A {type.ToString().ToLower()} drop event has been triggered \nClick the {claim} reaction on this message to claim it!",
                Attachments = null,
                Embeds = null,
                AllowedMentions = LocalAllowedMentions.None,
                Reference = new LocalMessageReference()
                    {GuildId = guild.Id, ChannelId = msg.ChannelId, MessageId = msg.Id, FailOnUnknownMessage = false},
                IsTextToSpeech = false
            });
            var emotes = GetEmotes(guild);
            foreach (var x in emotes.OrderBy(_ => _random.Next()).Take(emotes.Count))
                await ApplyReactionAsync(triggerMsg, x, claim, type);

            _logger.Log(LogLevel.Info, $"Drop event created in {guildId}");
        }

        private async Task ClaimAsync(IUserMessage msg, IMember user, DropType type)
        {
            try
            {
                await msg.DeleteAsync();
            }
            catch
            {
                /* Ignore */
            }

            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var rand = type == DropType.Regular
                ? _random.Next(15, 150)
                : _random.Next(150, 250);

            var userData = await db.GetOrCreateUserData(user);
            var cfg = await db.GetOrCreateCurrencyConfigAsync(user.GuildId);
            var exp = await _exp.AddExpAsync(user, userData, rand, rand, db, ExpSource.Other);
            var trgMsg = await _bot.SendMessageAsync(msg.ChannelId, new LocalMessage
            {
                Content =
                    $"Rewarded {user.Mention} with {exp} experience & {cfg.ToCurrencyFormat(rand)} {cfg.CurrencyName}!",
                Attachments = null,
                Embeds = null,
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false,
                Reference = null
            });

            try
            {
                await Task.Delay(5000);
                await trgMsg.DeleteAsync();
            }
            catch
            {
                /* Ignore */
            }

            await _achievement.DropAchievement(userData, db);
        }

        private async ValueTask<IEmoji> GetClaimEmoteAsync(IGuild guild, DbService db)
        {
            if (_cache.TryGetEmote(EmoteType.Drop, guild.Id, out var emote)) return emote;
            var cfg = await db.GetOrCreateDropConfigAsync(guild);
            if (TryParse(cfg.Emote, out var sEmote))
            {
                _cache.AddOrUpdateEmote(EmoteType.Drop, guild.Id, sEmote);
                return sEmote;
            }

            var defaultEmote = new LocalEmoji("U+1F381");
            _cache.AddOrUpdateEmote(EmoteType.Drop, guild.Id, defaultEmote);
            return defaultEmote;
        }

        private List<IGuildEmoji> GetEmotes(IGuild guild)
        {
            return guild.Emojis.Count >= 4
                ? guild.Emojis.Values.ToList()
                : _bot.GetGuild(431617676859932704).Emojis.Values.ToList();
        }

        private async Task ApplyReactionAsync(IMessage message, IGuildEmoji emote, IEmoji claim, DropType type)
        {
            if (emote.Name == claim.Name) _cache.AddDrop(message.ChannelId, message.Id, type);
            await message.AddReactionAsync(LocalEmoji.FromEmoji(emote));
        }
    }
}