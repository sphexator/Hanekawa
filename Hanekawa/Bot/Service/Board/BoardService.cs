using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Board
{
    public class BoardService : INService
    {
        private readonly Hanekawa _bot;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;
        private readonly CacheService _cache;
        // TODO: Change name check on emotes to IDs and only allow emotes within that guild to be used
        // Favor the default star emote
        public BoardService(Hanekawa bot, IServiceProvider provider, CacheService cache)
        {
            _bot = bot;
            _provider = provider;
            _cache = cache;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task ReactionReceivedAsync(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (_cache.TryGetEmote(EmoteType.Board, e.GuildId.Value, out var emote) && e.Emoji.Name != emote.Name) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            if (emote == null)
            {
                emote = await UpdateEmoteAsync(e.GuildId.Value, db);
                if (e.Emoji.Name != emote.Name) return;
            }

            var cfg = await db.GetOrCreateBoardConfigAsync(e.GuildId.Value);
            var stat = await db.GetOrCreateBoardAsync(e.GuildId.Value, e.Message);
            var giver = await db.GetOrCreateUserData(e.Member);
            var receiver = await db.GetOrCreateUserData(e.GuildId.Value.RawValue, e.Message.Author.Id.RawValue);
            receiver.StarReceived++;
            giver.StarGiven++;
            stat.StarAmount++;
            await db.SaveChangesAsync();

            var cache = _cache.Board.GetOrAdd(e.GuildId.Value, new MemoryCache(new MemoryCacheOptions()));
            if (cache.TryGetValue(e.MessageId, out var value)) 
                cache.Set(e.MessageId, (int) value + 1, TimeSpan.FromMinutes(10));
            else 
                cache.Set(e.MessageId, stat.StarAmount, TimeSpan.FromMinutes(10));
            
            if (stat.StarAmount >= 4 && !stat.Boarded.HasValue)
            {
                stat.Boarded = new DateTimeOffset(DateTime.UtcNow);
                await db.SaveChangesAsync();
                await SendMessageAsync(await _bot.GetOrFetchMemberAsync(e.GuildId.Value, e.Message.Author.Id), e.Message, cfg);
            }
        }

        public async Task ReactionRemovedAsync(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (_cache.TryGetEmote(EmoteType.Board, e.GuildId.Value, out var emote) && e.Emoji.Name != emote.Name) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            if (emote == null)
            {
                emote = await UpdateEmoteAsync(e.GuildId.Value, db);
                if (e.Emoji.Name != emote.Name) return;
            }
            
            var stat = await db.GetOrCreateBoardAsync(e.GuildId.Value, e.Message);
            var giver = await db.GetOrCreateUserData(e.GuildId.Value, e.UserId);
            var receiver = await db.GetOrCreateUserData(e.GuildId.Value.RawValue, e.Message.Author.Id.RawValue);
            if(receiver.StarReceived != 0) receiver.StarReceived--;
            giver.StarGiven--;
            stat.StarAmount--;
            await db.SaveChangesAsync();

            var cache = _cache.Board.GetOrAdd(e.GuildId.Value, new MemoryCache(new MemoryCacheOptions()));
            if (cache.TryGetValue(e.MessageId, out var value)) 
                cache.Set(e.MessageId, (int) value + 1, TimeSpan.FromMinutes(10));
            else 
                cache.Set(e.MessageId, stat.StarAmount, TimeSpan.FromMinutes(10));
        }

        public async Task ReactionClearedAsync(ReactionsClearedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            var messages = _cache.Board.GetOrAdd(e.GuildId.Value, new MemoryCache(new MemoryCacheOptions()));
            
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var stat = await db.GetOrCreateBoardAsync(e.GuildId.Value, e.Message);
            stat.StarAmount = 0;
            messages.Remove(e.MessageId);
            await db.SaveChangesAsync();
        }

        private static async Task<IEmoji> UpdateEmoteAsync(Snowflake guildId, DbService db)
        {
            var cfg = await db.GetOrCreateBoardConfigAsync(guildId.RawValue);
            return LocalCustomEmoji.TryParse(cfg.Emote, out var emoji) 
                ? emoji 
                : new LocalEmoji("U+2B50");
        }

        private async Task SendMessageAsync(IMember user, IUserMessage msg, BoardConfig cfg)
        {
            var roles = user.GetRoles();
            var guild = _bot.GetGuild(user.GuildId);
            var channel = guild.GetChannel(msg.ChannelId);
            if (!cfg.Channel.HasValue) return;
            var boardCh = guild.GetChannel(cfg.Channel.Value);
            if (boardCh == null) return;
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = user.Nick ?? user.Name,
                    IconUrl = user.GetAvatarUrl() ?? msg.Author.GetAvatarUrl()
                },
                Color = roles.Values.OrderByDescending(x => x.Position)
                    .FirstOrDefault(x => x.Color != null && x.Color.Value != 0)
                    ?.Color,
                Description = msg.Content,
                Footer = new LocalEmbedFooterBuilder {Text = channel?.Name},
                Timestamp = msg.CreatedAt
            };
            if (msg.Attachments.Count > 0) embed.ImageUrl = msg.Attachments[0].Url;
            embed.AddField("Original", $"[Jump!]({Discord.MessageJumpLink(user.GuildId, msg.ChannelId, msg.Id)})");
            
            await _bot.SendMessageAsync(boardCh.Id, new LocalMessageBuilder
            {
                Attachments = null,
                Content = null,
                Embed = embed,
                Mentions = LocalMentionsBuilder.None,
                Reference = new LocalReferenceBuilder
                {
                    ChannelId = msg.ChannelId,
                    GuildId = user.GuildId,
                    MessageId = msg.Id,
                    FailOnInvalid = false
                },
                IsTextToSpeech = false
            }.Build());
        }
    }
}