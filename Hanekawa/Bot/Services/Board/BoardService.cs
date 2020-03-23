using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService : INService, IRequired
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;

        public BoardService(DiscordBot client, InternalLogService log)
        {
            _client = client;
            _log = log;

            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;
            _client.ReactionsCleared += ReactionsClearedAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.BoardConfigs) _reactionEmote.TryAdd(x.GuildId, x.Emote ?? "⭐");
            }
        }

        private Task ReactionAddedAsync(ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Channel is CachedTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(e.User.Value is CachedMember user)) return;
                if (user.IsBot) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var emote = await GetEmote(user.Guild, db);
                        if (!e.Emoji.Equals(emote)) return;
                        var cfg = await db.GetOrCreateBoardConfigAsync(ch.Guild);
                        if (!cfg.Channel.HasValue) return;

                        var stat = await db.GetOrCreateBoard(ch.Guild, e.Message.Value);
                        var giver = await db.GetOrCreateUserData(user);
                        var receiver = await db.GetOrCreateUserData(e.Message.Value.Author as CachedMember);
                        receiver.StarReceived++;
                        giver.StarGiven++;
                        stat.StarAmount++;
                        await db.SaveChangesAsync();
                        IncreaseReactionAmount(user.Guild, e.Message.Value);
                        if (GetReactionAmount(user.Guild, e.Message.Value) >= 4 && !stat.Boarded.HasValue)
                        {
                            stat.Boarded = new DateTimeOffset(DateTime.UtcNow);
                            await db.SaveChangesAsync();
                            await SendMessageAsync(user, e.Message.Value as CachedUserMessage, cfg);
                        }
                        _log.LogAction(LogLevel.Information, $"(Board Service) {user.Id} added a reaction in {user.Guild.Id}");
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Board Service) Error in {ch.Guild.Id} for Reaction Added - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task ReactionRemovedAsync(ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Channel is CachedTextChannel ch)) return;
                if (ch.IsNsfw) return;
                if (!(e.User.Value is CachedMember user)) return;
                if (user.IsBot) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var emote = await GetEmote(user.Guild, db);
                        if (!e.Emoji.Equals(emote)) return;
                        var stat = await db.GetOrCreateBoard(ch.Guild, e.Message.Value);
                        var giver = await db.GetOrCreateUserData(user);
                        var receiver = await db.GetOrCreateUserData(e.Message.Value.Author as CachedMember);
                        receiver.StarReceived--;
                        giver.StarGiven--;
                        stat.StarAmount--;
                        await db.SaveChangesAsync();
                        DecreaseReactionAmount(user.Guild, e.Message.Value);
                        _log.LogAction(LogLevel.Information, $"(Board Service) {user.Id} removed a reaction in {user.Guild.Id}");
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Board Service) Error in {ch.Guild.Id} for Reaction Removed - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task ReactionsClearedAsync(ReactionsClearedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (!(e.Channel is CachedTextChannel ch)) return;
                var msgCheck = _reactionMessages.TryGetValue(ch.Guild.Id, out var messages);
                if (!msgCheck) return;
                if (messages.TryGetValue(e.Message.Id, out _)) messages.Remove(e.Message.Id);
            });
            return Task.CompletedTask;
        }

        private async Task<RestUserMessage> SendMessageAsync(CachedMember rctUser, CachedUserMessage msg, BoardConfig cfg)
        {
            var user = msg.Author as CachedMember;
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = $"{user.DisplayName} (Jump!)",
                    Url = user.GetAvatarUrl(),
                    IconUrl = msg.JumpUrl
                },
                Color = user.Roles.OrderByDescending(x => x.Value.Position)
                    .FirstOrDefault(x => x.Value.Color != null && x.Value.Color.Value != 0).Value.Color,
                Description = msg.Content,
                Footer = new LocalEmbedFooterBuilder {Text = msg.Channel.Name},
                Timestamp = msg.CreatedAt
            };
            if (msg.Attachments.Count > 0) embed.ImageUrl = msg.Attachments.First().Url;
            if (!cfg.Channel.HasValue) return null;
            var channel = rctUser.Guild.GetTextChannel(cfg.Channel.Value);
            return await channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}