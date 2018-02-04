using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Loot
{
    public class LootCrates
    {
        private readonly List<ulong> _crateMessage = new List<ulong>();
        private readonly List<ulong> _lootChannels = new List<ulong>();
        private readonly DiscordSocketClient _client;

        public LootCrates(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += CrateTrigger;
            _client.ReactionAdded += CrateClaimer;
            _lootChannels.Add(404633037884620802);
            _lootChannels.Add(404633067966300170);
        }

        private Task CrateTrigger(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;

                var rand = new Random();
                var chance = rand.Next(0, 1000);
                Console.Write(chance);
                if (chance < 300)
                {
                    var ch = message.Channel as SocketGuildChannel;
                    var triggerMsg = await (ch as SocketTextChannel)?.SendMessageAsync(
                        "A loot crate has dropped \nReact to this message to claim it");
                    Emote.TryParse("<:zulul:382923660174032906>", out var emote);
                    IEmote iemoteYes = emote;
                    await triggerMsg.AddReactionAsync(iemoteYes);
                    _crateMessage.Add(triggerMsg.Id);
                }
            });
            return Task.CompletedTask;
        }

        private Task CrateClaimer(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                if (msg.Value.Author.IsBot != true) return;
                if (reaction.User.Value.IsBot) return;
                if (!_lootChannels.Contains(msg.Value.Channel.Id)) return;
                if (reaction.Emote.Name != "zulul") return;
                Console.WriteLine("Reaction passed");

                var message = await msg.GetOrDownloadAsync();
                await message.DeleteAsync();
                var user = reaction.User.Value;
                var rand = new Random();
                var reward = rand.Next(25, 250);
                await ch.SendMessageAsync($"rewarded {user.Mention} with {reward} exp and credit");
            });
            return Task.CompletedTask;
        }
    }
}
