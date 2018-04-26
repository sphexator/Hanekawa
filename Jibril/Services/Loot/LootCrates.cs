﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.Level;
using Jibril.Services.Level.Services;

namespace Jibril.Services.Loot
{
    public class LootCrates
    {
        private readonly List<ulong> _crateMessage = new List<ulong>();
        private readonly List<ulong> _sCMessage = new List<ulong>();
        private readonly List<ulong> _lootChannels = new List<ulong>();

        private List<CooldownUser> _users = new List<CooldownUser>();
        private readonly DiscordSocketClient _client;

        public LootCrates(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += CrateTrigger;
            _client.ReactionAdded += CrateClaimer;
            _lootChannels.Add(339371997802790913); //General
            _lootChannels.Add(351861569530888202); //Tea-room
            _lootChannels.Add(341904875363500032); //Gaming
            _lootChannels.Add(353306001858101248); //Anime

            _lootChannels.Add(404633037884620802); //Test channel
        }

        private Task CrateTrigger(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!_lootChannels.Contains(msg.Channel.Id)) return;
                var cd = CheckCooldownAsync(msg.Author as SocketGuildUser);
                if (cd == false) return;
                try
                {
                    var rand = new Random();
                    var chance = rand.Next(0, 10000);
                    if (chance < 200)
                    {
                        var ch = message.Channel as SocketGuildChannel;
                        var triggerMsg = await (ch as SocketTextChannel)?.SendMessageAsync(
                            "A drop event has been triggered \nClick the roosip reaction on this message to claim it");
                        var emotes = ReturnEmotes();
                        var rng = new Random();
                        foreach (var x in emotes.OrderBy(x => rng.Next()).Take(4))
                        {
                            await Task.Delay(1000);
                            if (x.Name == "realsip") _crateMessage.Add(triggerMsg.Id);
                            await triggerMsg.AddReactionAsync(x);
                        }
                    }
                }
                catch
                {
                    //ignore
                }
            });
            return Task.CompletedTask;
        }

        public async Task SpawnCrate(SocketTextChannel ch, SocketGuildUser user)
        {
            try
            {
                var triggerMsg = await ch.SendMessageAsync(
                    $"```{user.Username} has spawned a crate! \nClick the reaction on this message to claim it```");
                var emotes = ReturnEmotes();
                var rng = new Random();
                foreach (var x in emotes.OrderBy(x => rng.Next()).Take(8))
                {
                    await Task.Delay(1000);
                    if (x.Name == "realsip") _sCMessage.Add(triggerMsg.Id);
                    await triggerMsg.AddReactionAsync(x);
                }
            }
            catch
            {
                //ignore
            }
        }

        private Task CrateClaimer(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                if (msg.Value.Author.IsBot != true) return;
                if (reaction.User.Value.IsBot) return;
                if (reaction.Emote.Name != "realsip") return;
                Console.WriteLine("Reaction passed");
                try
                {
                    if (_crateMessage.Contains(msg.Id))
                    {
                        var message = await msg.GetOrDownloadAsync();
                        await message.DeleteAsync();
                        var user = reaction.User.Value;
                        var rand = new Random();
                        var reward = rand.Next(15, 150);
                        var triggermsg = await ch.SendMessageAsync($"rewarded {user.Mention} with {reward} exp and credit");
                        LevelDatabase.AddExperience(user, reward, reward);
                        await Task.Delay(5000);
                        await triggermsg.DeleteAsync();
                    }
                    if (_sCMessage.Contains(msg.Id))
                    {
                        var message = await msg.GetOrDownloadAsync();
                        await message.DeleteAsync();
                        var user = reaction.User.Value;
                        var rand = new Random();
                        var reward = rand.Next(150, 250);
                        var triggermsg = await ch.SendMessageAsync($"rewarded {user.Mention} with {reward} exp and credit");
                        LevelDatabase.AddExperience(user, reward, reward);
                        await Task.Delay(5000);
                        await triggermsg.DeleteAsync();
                    }
                }
                catch
                {
                    //Ignore
                }
            });
            return Task.CompletedTask;
        }

        private IEnumerable<IEmote> ReturnEmotes()
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

        private bool CheckCooldownAsync(SocketGuildUser usr)
        {
            var tempUser = _users.FirstOrDefault(x => x.User == usr);
            if (tempUser != null)// check to see if you have handled a request in the past from this user.
            {
                if (!((DateTime.Now - tempUser.LastRequest).TotalSeconds >= 60)) return false;
                _users.Find(x => x.User == usr).LastRequest = DateTime.Now; // update their last request time to now.
                return true;

            }

            var newUser = new CooldownUser
            {
                User = usr,
                LastRequest = DateTime.Now
            };
            _users.Add(newUser);
            return true;
        }
    }
}
