using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Account;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.WebUI.Extensions;
using Hanekawa.WebUI.Bot.Service.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;

namespace Hanekawa.WebUI.Bot.Service.Board
{
    public class BoardService : DiscordClientService
    {
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;
        private readonly Logger _logger;

        private readonly IServiceProvider _provider;

        // TODO: Change name check on emotes to IDs and only allow emotes within that guild to be used
        // Favor the default star emote
        public BoardService(Hanekawa bot, IServiceProvider provider, CacheService cache, ILogger<BoardService> logger) :
            base(logger, bot)
        {
            _bot = bot;
            _provider = provider;
            _cache = cache;
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (_cache.TryGetEmote(EmoteType.Board, e.GuildId.Value, out var emote) && (e.Emoji.Name != emote.Name ||
                e.Emoji.GetId().HasValue && e.Emoji.GetId() != emote.GetId())) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            if (emote == null)
            {
                emote = await UpdateEmoteAsync(e.GuildId.Value, db);
                if (e.Emoji.Name != emote.Name) return;
            }

            var cfg = await db.GetOrCreateEntityAsync<BoardConfig>(e.GuildId.Value);
            var stat = await db.GetOrCreateEntityAsync<global::Hanekawa.Entities.BoardConfig.Board>(e.GuildId.Value, e.MessageId);
            var giver = await db.GetOrCreateEntityAsync<Account>(e.GuildId.Value, e.UserId);
            var receiver = await db.GetOrCreateEntityAsync<Account>(e.GuildId.Value, e.Message.Author.Id);
            receiver.StarReceived++;
            giver.StarGiven++;
            stat.StarAmount++;
            await db.SaveChangesAsync();

            if (stat.StarAmount >= 4 && !stat.Boarded.HasValue)
            {
                stat.Boarded = new DateTimeOffset(DateTime.UtcNow);
                await db.SaveChangesAsync();
                await SendMessageAsync(await _bot.GetOrFetchMemberAsync(e.GuildId.Value, e.Message.Author.Id),
                    e.Message, cfg, db);
                _logger.Info($"Sent board message in {cfg.GuildId} by user {receiver.UserId}");
            }
        }

        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (_cache.TryGetEmote(EmoteType.Board, e.GuildId.Value, out var emote) &&
                e.Emoji.Name != emote.Name) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            if (emote == null)
            {
                emote = await UpdateEmoteAsync(e.GuildId.Value, db);
                if (e.Emoji.Name != emote.Name) return;
            }

            var stat = await db.GetOrCreateEntityAsync<global::Hanekawa.Entities.BoardConfig.Board>(e.GuildId.Value, e.MessageId);
            var giver = await db.GetOrCreateEntityAsync<Account>(e.GuildId.Value, e.UserId);
            var receiver = await db.GetOrCreateEntityAsync<Account>(e.GuildId.Value, e.Message.Author.Id);
            if (receiver.StarReceived != 0) receiver.StarReceived--;
            if (giver.StarGiven != 0) giver.StarGiven--;
            if (stat.StarAmount != 0) stat.StarAmount--;
            await db.SaveChangesAsync();
        }

        protected override async ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var stat = await db.GetOrCreateEntityAsync<global::Hanekawa.Entities.BoardConfig.Board>(e.GuildId.Value, e.MessageId);
            db.Remove(stat);
            await db.SaveChangesAsync();
        }

        private static async Task<IEmoji> UpdateEmoteAsync(Snowflake guildId, DbService db)
        {
            var cfg = await db.GetOrCreateEntityAsync<BoardConfig>(guildId);
            return LocalCustomEmoji.TryParse(cfg.Emote, out var emoji)
                ? emoji
                : new LocalEmoji("U+2B50");
        }

        private async Task SendMessageAsync(IMember user, IUserMessage msg, BoardConfig cfg, DbContext db)
        {
            var roles = user.GetRoles();
            var guild = _bot.GetGuild(user.GuildId);
            var channel = guild.GetChannel(msg.ChannelId) as ITextChannel;
            if (!cfg.Channel.HasValue) return;
            if (guild.GetChannel(cfg.Channel.Value) is not ITextChannel boardCh) return;
            var client = await boardCh.GetOrCreateWebhookClientAsync();
            if (!cfg.WebhookId.HasValue || cfg.WebhookId.Value != client.Id)
            {
                cfg.WebhookId = client.Id;
                cfg.Webhook = client.Token;
                await db.SaveChangesAsync();
            }

            await client.ExecuteAsync(new LocalWebhookMessage
            {
                Name = user.DisplayName(),
                AllowedMentions = LocalAllowedMentions.None,
                AvatarUrl = user.GetAvatarUrl(),
                IsTextToSpeech = false,
                Attachments = null,
                Content = null,
                Embeds = new LocalEmbed[]
                {
                    new()
                    {
                        Author = new LocalEmbedAuthor
                        {
                            Name = user.DisplayName(),
                            IconUrl = user.GetAvatarUrl() ?? msg.Author.GetAvatarUrl()
                        },
                        Color = roles.Values.OrderByDescending(x => x.Position)
                            .FirstOrDefault(x => x.Color != null && x.Color.Value != 0)
                            ?.Color,
                        Description = msg.Content,
                        Footer = new LocalEmbedFooter {Text = channel?.Name},
                        Timestamp = msg.CreatedAt(),
                        ImageUrl = GetAttachmentAsync(msg, channel, boardCh)
                    }
                }
            });
        }

        private static string GetAttachmentAsync(IUserMessage message, ITextChannel source, ITextChannel destination)
        {
            if (source.IsNsfw && !destination.IsNsfw) return null;
            var attachment = message.Attachments[0];
            if (attachment == null) return null;
            return !attachment.Url.IsPictureUrl()
                ? null
                : attachment.Url;
        }
    }
}