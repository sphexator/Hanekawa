using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
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
        private readonly DiscordSocketClient _client;
        private readonly ExpService _expService;
        private readonly InternalLogService _log;
        private readonly Random _random;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        public DropService(DiscordSocketClient client, Random random, ExpService expService, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _random = random;
            _expService = expService;
            _log = log;
            _provider = provider;
            _colourService = colourService;

            _client.MessageReceived += DropChance;
            _client.ReactionAdded += OnReactionAdded;

            using (var db = new DbService())
            {
                foreach (var x in db.DropConfigs)
                {
                    var emote = Emote.TryParse(x.Emote, out var result) ? result : GetDefaultEmote();
                    _emotes.TryAdd(x.GuildId, emote);
                }

                foreach (var x in db.LootChannels)
                {
                    var channels = _lootChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, bool>());
                    channels.TryAdd(x.ChannelId, true);
                }
            }
        }

        public async Task SpawnAsync(HanekawaContext context)
        {
            using (var db = new DbService())
            {
                var claim = await GetClaimEmote(context.Guild, db);
                var triggerMsg = await context.Channel.ReplyAsync(
                    $"{context.User.GetName()} has spawned a crate! \nClick {claim} reaction on this message to claim it```",
                    _colourService.Get(context.Guild.Id));
                var emotes = await ReturnEmotes(context.Guild, db);
                foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                    try
                    {
                        if (x.Id == claim.Id)
                        {
                            var messages = _normalLoot.GetOrAdd(context.Guild.Id,
                                new MemoryCache(new MemoryCacheOptions()));
                            messages.Set(triggerMsg.Id, false, TimeSpan.FromHours(1));
                        }

                        await triggerMsg.AddReactionAsync(x);
                    }
                    catch
                    {
                        break;
                    }
            }
        }

        private Task DropChance(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (msg.Author.IsBot) return;
                if (!(msg.Channel is SocketTextChannel ch)) return;
                if (!IsDropChannel(ch)) return;
                if (OnGuildCooldown(ch.Guild)) return;
                if (OnUserCooldown(user)) return;
                var rand = _random.Next(0, 10000);
                if (rand < 200)
                    try
                    {
                        using (var db = new DbService())
                        {
                            var claim = await GetClaimEmote(ch.Guild, db);
                            var triggerMsg = await ch.SendMessageAsync(
                                $"A drop event has been triggered \nClick the {claim} reaction on this message to claim it!");
                            var emotes = await ReturnEmotes(ch.Guild, db);
                            foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                            {
                                if (x.Id == claim.Id)
                                {
                                    var messages = _normalLoot.GetOrAdd(ch.Guild.Id,
                                        new MemoryCache(new MemoryCacheOptions()));
                                    messages.Set(triggerMsg.Id, false, TimeSpan.FromHours(1));
                                }

                                await triggerMsg.AddReactionAsync(x);
                            }
                        }

                        _log.LogAction(LogLevel.Information, null, $"(Drop Service) Drop event created in {user.Guild.Id}");
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e,
                            $"(Drop Service) Error in {user.Guild.Id} for drop create - {e.Message}");
                    }
            });
            return Task.CompletedTask;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chx, SocketReaction rct)
        {
            var _ = Task.Run(async () =>
            {
                if (!msg.HasValue) return;
                if (!(chx is SocketTextChannel channel)) return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (user.IsBot) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var claim = await GetClaimEmote(user.Guild, db);
                        if (rct.Emote.Name != claim.Name) return;
                        if (!IsDropMessage(user.Guild.Id, msg.Id, out var special)) return;
                        var message = await msg.GetOrDownloadAsync();
                        if (special) await ClaimSpecial(message, channel, user, db);
                        else await ClaimNormal(message, channel, user, db);
                    }

                    _log.LogAction(LogLevel.Information, null, $"(Drop Service) Drop event claimed by {user.Id} in {user.Guild.Id}");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Drop Service) Error in {user.Guild.Id} for drop claim - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task ClaimSpecial(IMessage msg, SocketTextChannel channel, SocketGuildUser user, DbService db)
        {
            await msg.TryDeleteMessageAsync();
            var loots = _spawnedLoot.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id);
            var rand = _random.Next(150, 250);
            var userdata = await db.GetOrCreateUserData(user);
            await _expService.AddExpAsync(user, userdata, rand, rand, db);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.TryDeleteMessageAsync();
        }

        private async Task ClaimNormal(IMessage msg, SocketTextChannel channel, SocketGuildUser user, DbService db)
        {
            await msg.TryDeleteMessageAsync();
            var loots = _normalLoot.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id);
            var rand = _random.Next(15, 150);
            var userdata = await db.GetOrCreateUserData(user);
            await _expService.AddExpAsync(user, userdata, rand, rand, db);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.TryDeleteMessageAsync();
        }
    }
}