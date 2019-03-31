using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly ExpService _expService;
        private readonly Random _random;

        public DropService(DiscordSocketClient client, DbService db, Random random, ExpService expService)
        {
            _client = client;
            _db = db;
            _random = random;
            _expService = expService;

            _client.MessageReceived += DropChance;
            _client.ReactionAdded += OnReactionAdded;

            foreach (var x in _db.DropConfigs)
            {
                var emote = Emote.TryParse(x.Emote, out var result) ? result : GetDefaultEmote();
                _emotes.TryAdd(x.GuildId, emote);
            }

            foreach (var x in _db.LootChannels)
            {
                var channels = _lootChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, bool>());
                channels.TryAdd(x.ChannelId, true);
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
                {
                    var claim = await GetClaimEmote(ch.Guild);
                    var triggerMsg = await ch.SendMessageAsync(
                        $"A drop event has been triggered \nClick the {claim} reaction on this message to claim it!");
                    var emotes = await ReturnEmotes(ch.Guild);
                    foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                    {
                        if (x.Id == claim.Id)
                        {
                            var messages = _normalLoot.GetOrAdd(ch.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
                            messages.Set(triggerMsg.Id, false, TimeSpan.FromHours(1));
                        }
                        await triggerMsg.AddReactionAsync(x);
                    }
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
                var claim = await GetClaimEmote(user.Guild);
                if (rct.Emote.Name != claim.Name) return;
                if (!IsDropMessage(user.Guild.Id, msg.Id, out var special)) return;
                
                var message = await msg.GetOrDownloadAsync();
                if (special) await ClaimSpecial(message, channel, user);
                else await ClaimNormal(message, channel, user);
            });
            return Task.CompletedTask;
        }

        private async Task ClaimSpecial(IMessage msg, SocketTextChannel channel, SocketGuildUser user)
        {
            await msg.DeleteAsync();
            var loots = _spawnedLoot.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id);
            var rand = _random.Next(150, 250);
            var userdata = await _db.GetOrCreateUserData(user);
            await _expService.AddExp(user, userdata, rand, rand);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.DeleteAsync();
        }

        private async Task ClaimNormal(IMessage msg, SocketTextChannel channel, SocketGuildUser user)
        {
            await msg.DeleteAsync();
            var loots = _normalLoot.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            loots.Remove(msg.Id);
            var rand = _random.Next(15, 150);
            var userdata = await _db.GetOrCreateUserData(user);
            await _expService.AddExp(user, userdata, rand, rand);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.DeleteAsync();
        }
    }
}