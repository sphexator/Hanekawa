using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Services.Reaction
{
    public class ReactionService
    {
        private readonly DiscordSocketClient _client;
        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> ReactionMessages { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        public ReactionService(DiscordSocketClient client, IServiceProvider provider)
        {
            _client = client;

            _client.ReactionAdded += BoardReactionAdded;
            _client.ReactionRemoved += BoardReactionRemoved;
        }

        private Task BoardReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (((ITextChannel)reaction.Channel).IsNsfw) return Task.CompletedTask;
            if (reaction.Emote.Name != "OwO") return Task.CompletedTask;
            if (reaction.Message.Value.Author.IsBot) return Task.CompletedTask;
            if (reaction.User.Value.IsBot || reaction.UserId == reaction.Message.Value.Author.Id)
                return Task.CompletedTask;
            var board = ReactionMessages.GetOrAdd(reaction.Channel.Id, new ConcurrentDictionary<ulong, uint>());
            board.TryGetValue(reaction.MessageId, out var msg);
            if (msg + 1 == 2)
            {
                var _ = Task.Run(async () =>
                    {
                        board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg + 99);
                        await SendBoardAsync((ITextChannel)reaction.Channel, reaction.MessageId);
                    });
                return Task.CompletedTask;
            }

            board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg + 1);
            return Task.CompletedTask;
        }

        private Task BoardReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (((ITextChannel)reaction.Channel).IsNsfw) return Task.CompletedTask;
            if (reaction.Emote.Name != "OwO") return Task.CompletedTask;
            if (reaction.User.Value.IsBot || reaction.UserId == reaction.Message.Value.Author.Id)
                return Task.CompletedTask;
            var board = ReactionMessages.GetOrAdd(reaction.Channel.Id, new ConcurrentDictionary<ulong, uint>());
            board.TryGetValue(reaction.MessageId, out var msg);
            if (msg + 1 >= 2) return Task.CompletedTask;
            board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg - 1);

            return Task.CompletedTask;
        }

        private async Task SendBoardAsync(ITextChannel channel, ulong messageId)
        {
            var guild = _client.GetGuild(channel.GuildId);
            var message = await channel.GetMessageAsync(messageId);
            var user = message.Author as SocketGuildUser;
            var channelz = guild.GetTextChannel(433412697913163796);
            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Name = user.Nickname ?? user.Username
            };
            var footer = new EmbedFooterBuilder
            {
                Text = channel.Name
            };
            var embed = new EmbedBuilder
            {
                Author = author,
                Footer = footer,
                Description = message.Content,
                Timestamp = message.Timestamp,
                Color = GetBoardColor(user).Color
            };
            await channelz.SendMessageAsync(null, false, embed.Build());
        }

        private static SocketRole GetBoardColor(SocketGuildUser user)
        {
            return user.Roles.FirstOrDefault(x => x.Color.RawValue != 0);
        }
    }
}