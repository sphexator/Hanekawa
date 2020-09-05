using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly ExpService _expService;
        private readonly InternalLogService _log;
        private readonly Random _random;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        public DropService(Hanekawa client, Random random, ExpService expService, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _random = random;
            _expService = expService;
            _log = log;
            _provider = provider;
            _colourService = colourService;

            _client.MessageReceived += DropChance;
            _client.ReactionAdded += OnReactionAdded;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.DropConfigs)
            {
                if (LocalCustomEmoji.TryParse(x.Emote, out var result))
                {
                    _emotes.TryAdd(x.GuildId, result);
                }
                else _emotes.TryAdd(x.GuildId, GetDefaultEmote());
            }

            foreach (var x in db.LootChannels)
            {
                var channels = _lootChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, bool>());
                channels.TryAdd(x.ChannelId, true);
            }
        }

        private Task OnEmoteUpdated(GuildEmojisUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if ((e.NewEmojis.Count + 1) == e.OldEmojis.Count) return;
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateDropConfig(e.Guild);
                    using var removed = e.OldEmojis.Except(e.NewEmojis).GetEnumerator();
                    var list = new List<CachedGuildEmoji>();
                    while (removed.MoveNext())
                    {
                        list.Add(removed.Current.Value);
                    }

                    if (list.Count == 0) return;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var x = list[i];
                        if (x.MessageFormat == cfg.Emote)
                        {
                            cfg.Emote = null;
                            await db.SaveChangesAsync();
                            _log.LogAction(LogLevel.Information, $"Removed drop emote from {x.Guild.Id} as it was deleted");
                            return;
                        }
                    }
                }
                catch (Exception exception)
                {
                    _log.LogAction(LogLevel.Error, exception, exception.Message);
                }
            });
            return Task.CompletedTask;
        }

        public async Task SpawnAsync(DiscordCommandContext context)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var claim = await GetClaimEmote(context.Guild, db);
            var triggerMsg = await context.Channel.ReplyAsync(
                $"{context.Member.DisplayName} has spawned a crate! \nClick {claim} reaction on this message to claim it```",
                _colourService.Get(context.Guild.Id.RawValue));
            var emotes = await ReturnEmotes(context.Guild, db);
            foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                try
                {
                    if (x.Name == claim.Name)
                    {
                        var messages = _spawnedLoot.GetOrAdd(context.Guild.Id.RawValue,
                            new MemoryCache(new MemoryCacheOptions()));
                        messages.Set(triggerMsg.Id.RawValue, false, TimeSpan.FromHours(1));
                    }

                    await triggerMsg.AddReactionAsync(x);
                }
                catch
                {
                    break;
                }
        }

        private Task DropChance(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Message.Author is CachedMember user)) return;
                if (user.IsBot) return;
                if (!(e.Message.Channel is CachedTextChannel ch)) return;
                if (!IsDropChannel(ch)) return;
                if (OnGuildCooldown(ch.Guild)) return;
                if (OnUserCooldown(user)) return;
                var rand = _random.Next(0, 10000);
                if (rand < 200)
                    try
                    {
                        using var scope = _provider.CreateScope();
                        await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                        var claim = await GetClaimEmote(ch.Guild, db);
                        var triggerMsg = await ch.SendMessageAsync(
                            $"A drop event has been triggered \nClick the {claim} reaction on this message to claim it!");
                        var emotes = await ReturnEmotes(ch.Guild, db);
                        foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                        {
                            if (x.Name == claim.Name)
                            {
                                var messages = _normalLoot.GetOrAdd(ch.Guild.Id.RawValue,
                                    new MemoryCache(new MemoryCacheOptions()));
                                messages.Set(triggerMsg.Id.RawValue, false, TimeSpan.FromHours(1));
                            }

                            await triggerMsg.AddReactionAsync(x);
                        }

                        _log.LogAction(LogLevel.Information, $"(Drop Service) Drop event created in {user.Guild.Id.RawValue}");
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e,
                            $"(Drop Service) Error in {user.Guild.Id.RawValue} for drop create - {e.Message}");
                    }
            });
            return Task.CompletedTask;
        }

        private Task OnReactionAdded(ReactionAddedEventArgs e)
        {
            var _ = Task.Run(async () =>
            {
                if (!(e.Channel is CachedTextChannel channel)) return;
                if (!e.User.HasValue) await e.User.GetAsync();
                if (!(e.User.Value is CachedMember user)) return;
                if (user.IsBot) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var check = _emotes.TryGetValue(user.Guild.Id.RawValue, out var claim);
                    if(!check) claim = await GetClaimEmote(user.Guild, db);
                    if (e.Emoji.MessageFormat != claim.MessageFormat) return;
                    if (!IsDropMessage(user.Guild.Id.RawValue, e.Message.Id.RawValue, out var special)) return;
                    var message = await e.Message.GetAsync();
                    if (special) await ClaimSpecial(message, channel, user, db);
                    else await ClaimNormal(message, channel, user, db);

                    _log.LogAction(LogLevel.Information, $"(Drop Service) Drop event claimed by {user.Id.RawValue} in {user.Guild.Id.RawValue}");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Drop Service) Error in {user.Guild.Id.RawValue} for drop claim - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task ClaimSpecial(IMessage msg, CachedTextChannel channel, CachedMember user, DbService db)
        {
            await msg.DeleteAsync();
            var loots = _spawnedLoot.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id.RawValue);
            var rand = _random.Next(150, 250);
            var userdata = await db.GetOrCreateUserData(user);
            var exp = await _expService.AddExpAsync(user, userdata, rand, rand, db);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {exp} exp & {rand} credit!");
            try
            {
                await Task.Delay(5000);
                await trgMsg.DeleteAsync();
            }
            catch
            {
                // Ignore
            }
        }

        private async Task ClaimNormal(IMessage msg, CachedTextChannel channel, CachedMember user, DbService db)
        {
            await msg.DeleteAsync();
            var loots = _normalLoot.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id.RawValue);
            var rand = _random.Next(15, 150);
            var userdata = await db.GetOrCreateUserData(user);
            var exp = await _expService.AddExpAsync(user, userdata, rand, rand, db);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {exp} exp & {rand} credit!");
            try
            {
                await Task.Delay(5000);
                await trgMsg.DeleteAsync();
            }
            catch
            {
                // Ignore
            }
        }
    }
}