using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public BoardService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            foreach (var x in _db.BoardConfigs)
            {
                _reactionEmote.TryAdd(x.GuildId, x.Emote ?? "⭐");
            }

            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;
            _client.ReactionsCleared += ReactionsClearedAsync;
        }

        private Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction rct)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is SocketTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (user.IsBot) return;
                var emote = await GetEmote(user.Guild);
                if (!rct.Emote.Equals(emote)) return;
            });
            return Task.CompletedTask;
        }

        private Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction rct)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is SocketTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (user.IsBot) return;
                var emote = await GetEmote(user.Guild);
                if (!rct.Emote.Equals(emote)) return;

            });
            return Task.CompletedTask;
        }

        private Task ReactionsClearedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is SocketTextChannel ch)) return;
                var msgCheck= _reactionMessages.TryGetValue(ch.Guild.Id, out var messages);
                if (!msgCheck) return;
                if (messages.TryGetValue(message.Id, out _))
                {
                    messages.Remove(message.Id);
                }
            });
            return Task.CompletedTask;
        }

        private async Task SendMessageAsync()
        {

        }
    }
}
