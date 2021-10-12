using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.AutoMessage
{
    public class AutoMessageService : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CacheService _cache;
        private readonly Hanekawa _bot;

        public AutoMessageService(ILogger<AutoMessageService> logger, DiscordClientBase client, IServiceProvider provider, CacheService cache) 
            : base(logger, client)
        {
            _provider = provider;
            _cache = cache;
            _bot = (Hanekawa) client;
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in e.GuildIds)
            {
                var messages = await db.AutoMessages.Where(autoMessage => autoMessage.GuildId == x).ToArrayAsync();
                foreach (var z in messages)
                {
                    var channel = _bot.GetChannel(z.GuildId, z.ChannelId) as ITextChannel;
                    _cache.AddOrUpdateAutoMessageTimer(z.GuildId, z.Name, CreateTimer(z, channel));
                }
            }
        }

        public async Task<bool> AddMessage(Snowflake guildId, Snowflake creator, TimeSpan interval, string name, string message, ITextChannel channel)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.AutoMessages.FindAsync(guildId, name);
            if (check != null) return false;
            var client = await channel.GetOrCreateWebhookClientAsync();
            var autoMessage = new Database.Tables.AutoMessage.AutoMessage
            {
                GuildId = guildId,
                Name = name,
                Message = message,
                Interval = interval,
                ChannelId = channel.Id,
                Webhook = client.Token,
                WebhookId = client.Id,
                Creator = creator
            };
            await db.AutoMessages.AddAsync(autoMessage);
            await db.SaveChangesAsync();
            _cache.AddOrUpdateAutoMessageTimer(guildId, name, CreateTimer(autoMessage, channel));
            return true;
        }

        public async Task<bool> Remove(Snowflake guildId, string name)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var autoMessage = await db.AutoMessages.FindAsync(guildId, name);
            if (autoMessage == null) return false;
            db.AutoMessages.Remove(autoMessage);
            _cache.RemoveAutoMessageTimer(guildId, name);
            await db.SaveChangesAsync();
            return true;
        }
        
        private Timer CreateTimer(Database.Tables.AutoMessage.AutoMessage config, ITextChannel textChannel)
        {
            TimeSpan? interval = null;
            var date = DateTime.UtcNow;
            var currentTime = TimeSpan.Zero;
            while (interval == null)
            {
                if(currentTime.Add(config.Interval) < date.TimeOfDay) continue;
                interval = currentTime - date.TimeOfDay;
            }

            var guild = _bot.GetGuild(config.GuildId);
            return new Timer(async _ =>
            {
                var client = await textChannel.GetOrCreateWebhookClientAsync();
                var builder = new LocalWebhookMessage
                {
                    Name = guild.Name,
                    AvatarUrl = guild.GetIconUrl(),
                    Embeds = new LocalEmbed[]
                    {
                        new()
                        {
                            Color = _provider.GetRequiredService<CacheService>().GetColor(config.GuildId),
                            Description = config.Message
                        }
                    }
                };
                await client.ExecuteAsync(builder);
            }, null, interval.Value, config.Interval);
        }
    }
}