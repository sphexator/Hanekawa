using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;

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
        
        private Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction rct)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is SocketTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (user.IsBot) return;
                var emote = await GetEmote(user.Guild);
                if (!rct.Emote.Equals(emote)) return;
                var cfg = await _db.GetOrCreateBoardConfigAsync(ch.Guild);
                if (!cfg.Channel.HasValue) return;
                
                var message = await msg.GetOrDownloadAsync();
                var stat = await _db.GetOrCreateBoard(ch.Guild, message);
                var giver = await _db.GetOrCreateUserData(user);
                var receiver = await _db.GetOrCreateUserData(message.Author as SocketGuildUser);
                receiver.StarReceived++;
                giver.StarGiven++;
                stat.StarAmount++;
                await _db.SaveChangesAsync();
                IncreaseReactionAmount(user.Guild, message);
                if (GetReactionAmount(user.Guild, message) >= 4 && !stat.Boarded.HasValue)
                {
                    stat.Boarded = new DateTimeOffset(DateTime.UtcNow);
                    await _db.SaveChangesAsync();
                    await SendMessageAsync(user, message, cfg);
                }
            });
            return Task.CompletedTask;
        }

        private Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction rct)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is SocketTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (user.IsBot) return;
                var emote = await GetEmote(user.Guild);
                if (!rct.Emote.Equals(emote)) return;
                
                var message = await msg.GetOrDownloadAsync(); 
                var stat = await _db.GetOrCreateBoard(ch.Guild, message);
                var giver = await _db.GetOrCreateUserData(user);
                var receiver = await _db.GetOrCreateUserData(message.Author as SocketGuildUser);
                receiver.StarReceived--;
                giver.StarGiven--;
                stat.StarAmount--;
                await _db.SaveChangesAsync();
                DecreaseReactionAmount(user.Guild, message);
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
        
        private async Task<RestUserMessage> SendMessageAsync(SocketGuildUser user, IUserMessage msg, BoardConfig cfg)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{user.GetName()} (Jump!)",
                    Url = user.GetAvatar(),
                    IconUrl = msg.GetJumpUrl()
                },
                Color = user.Roles.OrderByDescending(x => x.Position)
                    .FirstOrDefault(x => x.Color.RawValue != 0)?.Color,
                Description = msg.Content,
                Footer = new EmbedFooterBuilder { Text = msg.Channel.Name },
                Timestamp = msg.Timestamp,
            };
            if (msg.Attachments.Count > 0) embed.ImageUrl = msg.Attachments.First().Url;
            var channel = user.Guild.GetTextChannel(cfg.Channel.Value);
            return await channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}
