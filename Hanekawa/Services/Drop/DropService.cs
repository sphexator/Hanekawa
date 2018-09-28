using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;

namespace Hanekawa.Services.Drop
{
    public class DropService
    {
        private ConcurrentDictionary<ulong, List<ulong>> LootChannels { get; set; }
            = new ConcurrentDictionary<ulong, List<ulong>>();
        private ConcurrentDictionary<ulong, DateTime> GuildCooldown { get; set; }
            = new ConcurrentDictionary<ulong, DateTime>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> UserCooldown { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();

        private readonly List<ulong> _regularLoot = new List<ulong>();
        private readonly List<ulong> _specialLoot = new List<ulong>();

        private readonly DiscordSocketClient _client;

        public DropService(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += CrateTriggerAsync;
            _client.ReactionAdded += CrateClaimerAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var result = db.LootChannels.Where(c => c.GuildId == x.GuildId).ToList();
                    var channels = new List<ulong>();
                    foreach (var y in result)
                    {
                        channels.Add(y.ChannelId);
                    }
                    LootChannels.GetOrAdd(x.GuildId, channels);
                }
            }
        }

        public async Task SpawnCrateAsync(SocketTextChannel ch, SocketGuildUser user)
        {

            var triggerMsg = await ch.SendMessageAsync(
                $"```{user.Username} has spawned a crate! \nClick the reaction on this message to claim it```");
            var emotes = ReturnEmotes().ToList();
            var rng = new Random();
            foreach (var x in emotes.OrderBy(x => rng.Next()).Take(emotes.Count))
            {
                try
                {
                    if (x.Name == "realsip") _specialLoot.Add(triggerMsg.Id);
                    await triggerMsg.AddReactionAsync(x);
                }
                catch { break; }
            }
        }

        private Task CrateClaimerAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction rct)
        {
            var _ = Task.Run(async () =>
            {
                if (!msg.HasValue) return;
                if (rct.User.Value.IsBot) return;
                if (rct.Emote.Name != "realsip") return;
                if (!_regularLoot.Contains(rct.MessageId) &&
                    !_specialLoot.Contains(rct.MessageId)) return;
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(rct.User.Value as SocketGuildUser);
                    if (_specialLoot.Contains(rct.MessageId))
                    {
                        _specialLoot.Remove(rct.MessageId);
                        if (!msg.HasValue) await msg.GetOrDownloadAsync();
                        await msg.Value.DeleteAsync();
                        var rand = new Random().Next(150, 250);
                        userdata.Exp = userdata.Exp + Convert.ToUInt32(rand);
                        userdata.TotalExp = userdata.TotalExp + Convert.ToUInt32(rand);
                        userdata.Credit = userdata.Credit + Convert.ToUInt32(rand);
                        await db.SaveChangesAsync();
                        var trgMsg =
                            await channel.SendMessageAsync(
                                $"Rewarded {rct.User.Value.Mention} with {rand} exp & credit!");
                        await Task.Delay(5000);
                        await trgMsg.DeleteAsync();
                    }
                    else
                    {
                        _regularLoot.Remove(rct.MessageId);
                        if (!msg.HasValue) await msg.GetOrDownloadAsync();
                        await msg.Value.DeleteAsync();
                        var rand = new Random().Next(15, 150);
                        userdata.Exp = userdata.Exp + Convert.ToUInt32(rand);
                        userdata.TotalExp = userdata.TotalExp + Convert.ToUInt32(rand);
                        userdata.Credit = userdata.Credit + Convert.ToUInt32(rand);
                        await db.SaveChangesAsync();
                        var trgMsg = await channel.SendMessageAsync($"Rewarded {rct.User.Value.Mention} with {rand} exp & credit!");
                        await Task.Delay(5000);
                        await trgMsg.DeleteAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task CrateTriggerAsync(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!(msg.Channel is SocketTextChannel ch)) return;
                if (msg.Author.IsBot) return;
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!CheckUserCooldown(user)) return;
                if (!LootChannels.TryGetValue(ch.Guild.Id, out var chx)) return;
                if (!chx.Contains(msg.Channel.Id)) return;
                var rand = new Random().Next(0, 10000);
                if (rand < 200)
                {
                    if (!CheckGuildCooldown((SocketTextChannel)msg.Channel)) return;
                    var triggerMsg = await ch.SendMessageAsync(
                        "A drop event has been triggered \nClick the roosip reaction on this message to claim it!");
                    var emotes = ReturnEmotes().ToList();
                    foreach (var x in emotes.OrderBy(x => new Random().Next()).Take(emotes.Count))
                    {
                        if (x.Name == "realsip") _regularLoot.Add(triggerMsg.Id);
                        await triggerMsg.AddReactionAsync(x);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public async Task AddLootChannelAsync(SocketTextChannel ch)
        {
            await AddToDatabaseAsync(ch.Guild.Id, ch.Id);
            var channels = LootChannels.GetOrAdd(ch.Guild.Id, new List<ulong>());
            channels.Add(ch.Id);
            LootChannels.AddOrUpdate(ch.Guild.Id, channels, (key, old) => old = channels);
        }

        public async Task RemoveLootChannelAsync(SocketTextChannel ch)
        {
            await RemoveFromDatabaseAsync(ch.Guild.Id, ch.Id);
            if (!LootChannels.TryGetValue(ch.Guild.Id, out var channels)) return;
            if (!channels.Contains(ch.Id)) return;
            channels.Remove(ch.Id);
            LootChannels.AddOrUpdate(ch.Guild.Id, channels, (key, old) => channels);
        }

        private static async Task AddToDatabaseAsync(ulong guildId, ulong channelId)
        {
            using (var db = new DbService())
            {
                var data = new LootChannel
                {
                    ChannelId = channelId,
                    GuildId = guildId
                };
                await db.LootChannels.AddAsync(data);
                await db.SaveChangesAsync();
            }
        }

        private static async Task RemoveFromDatabaseAsync(ulong guildId, ulong channelId)
        {
            using (var db = new DbService())
            {
                var data = db.LootChannels.First(x => x.GuildId == guildId && x.ChannelId == channelId);
                db.LootChannels.Remove(data);
                await db.SaveChangesAsync();
            }
        }

        private static IEnumerable<Emote> ReturnEmotes()
        {
            var emotes = new List<Emote>();
            Emote.TryParse("<:realsip:429809346222882836>", out var real);
            Emote.TryParse("<:sip:430207651998334977>", out var sip1);
            Emote.TryParse("<:roowut:430207652061118465>", out var sip2);
            Emote.TryParse("<:rooWhine:430207965153460254>", out var sip3);

            emotes.Add(real);
            emotes.Add(sip1);
            emotes.Add(sip2);
            emotes.Add(sip3);

            return emotes;
        }

        private bool CheckGuildCooldown(SocketTextChannel guild)
        {
            var check = GuildCooldown.TryGetValue(guild.Id, out var cd);
            if (!check)
            {
                GuildCooldown.TryAdd(guild.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            GuildCooldown.AddOrUpdate(guild.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }

        private bool CheckUserCooldown(SocketGuildUser user)
        {
            var check = UserCooldown.TryGetValue(user.Guild.Id, out var cds);
            if (!check)
            {
                var guildCooldown = UserCooldown.GetOrAdd(user.Guild.Id, new ConcurrentDictionary<ulong, DateTime>());
                guildCooldown.TryAdd(user.Id, DateTime.UtcNow);
                return true;
            }

            var userCheck = cds.TryGetValue(user.Id, out var cd);
            if (!userCheck)
            {
                cds.TryAdd(user.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            cds.AddOrUpdate(user.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }
    }
}
