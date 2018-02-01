using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Loot
{
    public class LootCrates
    {
        private readonly DiscordSocketClient _discord;

        public LootCrates(DiscordSocketClient discord)
        {
            _discord = discord;

            _discord.MessageReceived += CrateTrigger;
            _discord.ReactionAdded += CrateClaimer;
        }

        private Task CrateTrigger(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;

                var rand = new Random();
                var chance = rand.Next(0, 100);
                if (chance < 20)
                {

                }
            });
            return Task.CompletedTask;
        }

        private Task CrateClaimer(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction reaction)
        {
            var _ = Task.Run(() =>
            {
                if (msg.Value.Author.IsBot != true) return;

            });
            return Task.CompletedTask;
        }
    }
}
