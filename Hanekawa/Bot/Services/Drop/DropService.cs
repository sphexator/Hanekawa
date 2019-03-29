using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly Random _random;

        public DropService(DiscordSocketClient client, DbService db, Random random)
        {
            _client = client;
            _db = db;
            _random = random;

            _client.MessageReceived += DropChance;
            _client.ReactionAdded += OnReactionAdded;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chx, SocketReaction rct)
        {
            _ = Task.Run(async () =>
            {
                if (!(chx is SocketTextChannel ch)) return;
                if (rct.User.GetValueOrDefault().IsBot) return;
                if (IsDropChannel(ch)) return;
                if (IsDropMessage(ch.Guild.Id, msg.Id, out var special)) return;
            });
            return Task.CompletedTask;
        }

        private Task DropChance(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if(!(msg.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                
            });
            return Task.CompletedTask;
        }
    }
}