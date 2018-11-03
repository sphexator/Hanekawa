using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Services.Reaction
{
    public class BoardService
    {
        private readonly DiscordSocketClient _client;

        public BoardService(DiscordSocketClient client, IServiceProvider provider)
        {
            _client = client;

            _client.ReactionAdded += BoardReactionAddedAsync;
            _client.ReactionRemoved += BoardReactionRemovedAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs) ReactionEmote.TryAdd(x.GuildId, x.BoardEmote ?? "⭐");
            }
        }

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> ReactionMessages { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        private ConcurrentDictionary<ulong, string> ReactionEmote { get; }
            = new ConcurrentDictionary<ulong, string>();

        public void SetBoardEmote(SocketGuild guild, string emote)
        {
            ReactionEmote.AddOrUpdate(guild.Id, emote, (key, old) => old = emote);
        }

        public IEmote GetGuildEmote(SocketGuild guild)
        {
            return GetEmote(guild);
        }

        private Task BoardReactionAddedAsync(Cacheable<IUserMessage, ulong> msges, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!(channel is ITextChannel ch)) return;
                    if (ch.IsNsfw) return;
                    if (reaction.User.IsSpecified && reaction.User.Value.IsBot) return;
                    if (msges.HasValue && msges.Value.Author.IsBot) return;
                    var message = await msges.GetOrDownloadAsync();
                    if (message.Author.IsBot) return;
                    if (message.Author.Id == reaction.UserId) return;

                    var emote = GetEmote((channel as SocketTextChannel)?.Guild);

                    if (!Equals(reaction.Emote, emote)) return;
                    using (var db = new DbService())
                    {
                        var stat = await db.GetOrCreateBoard(ch.Guild, message);
                        stat.StarAmount = stat.StarAmount + 1;

                        var giver = await db.GetOrCreateUserData(ch.Guild.Id, reaction.UserId);
                        var reciever = await db.GetOrCreateUserData(ch.Guild, message.Author);
                        giver.StarGiven = giver.StarGiven + 1;
                        reciever.StarReceived = reciever.StarReceived + 1;

                        var board = ReactionMessages.GetOrAdd(ch.Guild.Id, new ConcurrentDictionary<ulong, uint>());
                        var msg = board.GetOrAdd(message.Id, 0);
                        if (msg + 1 == 4 && !stat.Boarded.HasValue)
                        {
                            board.TryRemove(reaction.MessageId, out var value);
                            stat.Boarded = new DateTimeOffset(DateTime.UtcNow);
                            await db.SaveChangesAsync();
                            await SendBoardAsync(db, (ITextChannel) reaction.Channel, reaction.MessageId);
                        }
                        else
                        {
                            board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg + 1);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task BoardReactionRemovedAsync(Cacheable<IUserMessage, ulong> msges, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!(channel is ITextChannel ch)) return;
                    if (ch.IsNsfw) return;
                    if (reaction.User.IsSpecified && reaction.User.Value.IsBot) return;
                    if (msges.HasValue && msges.Value.Author.IsBot) return;
                    var message = await msges.GetOrDownloadAsync();
                    if (message.Author.IsBot) return;
                    if (message.Author.Id == reaction.UserId) return;

                    var emote = GetEmote((channel as SocketTextChannel)?.Guild);

                    if (!Equals(reaction.Emote, emote)) return;

                    using (var db = new DbService())
                    {
                        var stat = await db.GetOrCreateBoard(ch.Guild, message);
                        if ((int) stat.StarAmount - 1 < 0)
                            stat.StarAmount = 0;
                        else stat.StarAmount = stat.StarAmount - 1;

                        var giver = await db.GetOrCreateUserData(ch.Guild.Id, reaction.UserId);
                        var reciever = await db.GetOrCreateUserData(ch.Guild, message.Author);

                        if ((int) giver.StarGiven - 1 < 0)
                            giver.StarGiven = 0;
                        else giver.StarGiven = giver.StarGiven - 1;

                        if ((int) reciever.StarReceived - 1 < 0)
                            reciever.StarReceived = 0;
                        else reciever.StarReceived = reciever.StarReceived - 1;
                        await db.SaveChangesAsync();

                        var board = ReactionMessages.GetOrAdd(ch.Guild.Id, new ConcurrentDictionary<ulong, uint>());
                        if (board.TryGetValue(reaction.MessageId, out var msg))
                        {
                            if ((int) msg - 1 < 0) return;
                            board.AddOrUpdate(reaction.MessageId, 1, (key, old) => old = msg - 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            return Task.CompletedTask;
        }

        private async Task SendBoardAsync(DbService db, ITextChannel channel, ulong messageId)
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
                Color = GetBoardColor(user).Color
            };
            if (message.Attachments.Count > 0) embed.ImageUrl = message.Attachments.First().Url;
            await channelz.SendMessageAsync(null, false, embed.Build());
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

                if (Emote.TryParse(cfg.BoardEmote, out var result))
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