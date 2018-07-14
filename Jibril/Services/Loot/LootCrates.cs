using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jibril.Extensions;
using Jibril.Services.Entities;

namespace Jibril.Services.Loot
{
    public class LootCrates
    {
        private ConcurrentDictionary<ulong, ulong> LootChannels { get; set; }
            = new ConcurrentDictionary<ulong, ulong>();

        private readonly List<ulong> _regularLoot = new List<ulong>();
        private readonly List<ulong> _specialLoot = new List<ulong>();

        private readonly DiscordSocketClient _client;

        public LootCrates(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += CrateTrigger;
            _client.ReactionAdded += CrateClaimer;
        }

        private Task CrateClaimer(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction rct)
        {
            if (!msg.HasValue) return Task.CompletedTask;
            if (rct.User.Value.IsBot) return Task.CompletedTask;
            if (rct.Emote.Name != "realsip") return Task.CompletedTask;
            if (!_regularLoot.Contains(rct.MessageId) ||
                !_specialLoot.Contains(rct.MessageId)) return Task.CompletedTask;
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(rct.User.Value);
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
                        var trgMsg =
                            await channel.SendMessageAsync(
                                $"Rewarded {rct.User.Value.Mention} with {rand} exp & credit!");
                        await Task.Delay(5000);
                        await trgMsg.DeleteAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task CrateTrigger(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!LootChannels.TryGetValue(message.Channel.Id, out var chx)) return;

                var rand = new Random().Next(0, 10000);
                if (rand < 200)
                {
                    var ch = message.Channel as SocketTextChannel;
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

        public async Task SpawnCrate(SocketTextChannel ch, SocketGuildUser user)
        {

            var triggerMsg = await ch.SendMessageAsync(
                $"```{user.Username} has spawned a crate! \nClick the reaction on this message to claim it```");
            var emotes = ReturnEmotes();
            var rng = new Random();
            foreach (var x in emotes.OrderBy(x => rng.Next()).Take(8))
            {
                if (x.Name == "realsip") _specialLoot.Add(triggerMsg.Id);
                await triggerMsg.AddReactionAsync(x);
            }
        }

        private static IEnumerable<IEmote> ReturnEmotes()
        {
            var emotes = new List<IEmote>();
            Emote.TryParse("<:realsip:429809346222882836>", out var real);
            Emote.TryParse("<:sip:430207651998334977>", out var sip1);
            Emote.TryParse("<:roowut:430207652061118465>", out var sip2);
            Emote.TryParse("<:rooWhine:430207965153460254>", out var sip3);

            IEmote realEmote = real;
            IEmote fake1Emote = sip1;
            IEmote fake2Emote = sip2;
            IEmote fake3Emote = sip3;

            emotes.Add(realEmote);
            emotes.Add(fake1Emote);
            emotes.Add(fake2Emote);
            emotes.Add(fake3Emote);

            return emotes;
        }
    }
}
