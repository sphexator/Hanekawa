using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Services.Reaction
{
    public class BoardService
    {
        private readonly DiscordSocketClient _client;

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> ReactionMessages { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();
        private ConcurrentDictionary<ulong, string> ReactionEmote { get; }
            = new ConcurrentDictionary<ulong, string>();

        public BoardService(DiscordSocketClient client, IServiceProvider provider)
        {
            _client = client;

            _client.ReactionAdded += BoardReactionAddedAsync;
            _client.ReactionRemoved += BoardReactionRemovedAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    ReactionEmote.TryAdd(x.GuildId, x.BoardEmote ?? "⭐");
                }
            }
        }

        public void SetBoardEmote(SocketGuild guild, string emote)
        {
            ReactionEmote.AddOrUpdate(guild.Id, emote, (key, old) => old = emote);
        }

        private Task BoardReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                if (((ITextChannel)reaction.Channel).IsNsfw) return;
                if (message.Value.Author.IsBot) return;
                if (message.Value.Author.Id == reaction.UserId) return;
                var emote = GetEmote((channel as SocketTextChannel)?.Guild);
                if (!Equals(reaction.Emote, emote)) return;
                var board = ReactionMessages.GetOrAdd(reaction.Channel.Id, new ConcurrentDictionary<ulong, uint>());
                board.TryGetValue(reaction.MessageId, out var msg);
                if (msg + 1 == 4)
                {
                    board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg + 99);
                    await SendBoardAsync((ITextChannel)reaction.Channel, reaction.MessageId);
                    return;
                }

                board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg + 1);
            });
            return Task.CompletedTask;
        }

        private Task BoardReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var _ = Task.Run(() =>
            {
                if (((ITextChannel)reaction.Channel).IsNsfw) return;
                if (message.Value.Author.IsBot) return;
                if (message.Value.Author.Id == reaction.UserId) return;
                var emote = GetEmote((channel as SocketTextChannel)?.Guild);
                if (!Equals(reaction.Emote, emote)) return;

                var board = ReactionMessages.GetOrAdd(reaction.Channel.Id, new ConcurrentDictionary<ulong, uint>());
                board.TryGetValue(reaction.MessageId, out var msg);
                if (msg + 1 >= 4) return;
                board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg - 1);
            });

            return Task.CompletedTask;
        }

        private async Task SendBoardAsync(ITextChannel channel, ulong messageId)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(channel.Guild as SocketGuild);
                if (!cfg.BoardChannel.HasValue) return;
                var guild = _client.GetGuild(channel.GuildId);
                var message = await channel.GetMessageAsync(messageId);
                var user = message.Author as SocketGuildUser;
                var channelz = guild.GetTextChannel(cfg.BoardChannel.Value);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
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
                    Color = GetBoardColor(user).Color,
                };
                if (message.Attachments.Count > 0) embed.ImageUrl = message.Attachments.First().Url;
                await channelz.SendMessageAsync(null, false, embed.Build());
            }
        }

        private static SocketRole GetBoardColor(SocketGuildUser user)
        {
            return user.Roles.OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != 0);
        }

        private IEmote GetEmote(SocketGuild guild)
        {
            return GetDictionaryEmote(guild, out var emote) ? emote : GetDatabaseEmote(guild);
        }

        private bool GetDictionaryEmote(SocketGuild guild, out IEmote emote)
        {
            var check = ReactionEmote.TryGetValue(guild.Id, out var result);
            if (!check)
            {
                emote = null;
                return false;
            }

            if (Emote.TryParse(result, out var iemote))
            {
                emote = iemote;
                return true;
            }
            emote = new Emoji("⭐");
            return true;
        }

        private IEmote GetDatabaseEmote(SocketGuild guild)
        {
            using (var db = new DbService())
            {
                var cfg = db.GuildConfigs.Find(guild.Id);
                if (cfg.BoardEmote == null)
                {
                    ReactionEmote.TryAdd(guild.Id, "⭐");
                    return new Emoji("⭐");
                }

                if(Emote.TryParse(cfg.BoardEmote, out var result))
                {
                    ReactionEmote.TryAdd(guild.Id, cfg.BoardEmote);
                    return result;
                }

                ReactionEmote.TryAdd(guild.Id, "⭐");
                return new Emoji("⭐");
            }
        }
    }
}