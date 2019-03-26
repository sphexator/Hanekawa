using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Bot.Services.ImageGen;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly WelcomeImg _img;
        private readonly DbService _db;
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _cooldown 
            = new ConcurrentDictionary<ulong, MemoryCache>();
        private readonly ConcurrentDictionary<ulong, MemoryCache> _ratelimit 
            = new ConcurrentDictionary<ulong, MemoryCache>();
        
        public WelcomeService(DiscordSocketClient client, DbService db, WelcomeImg img)
        {
            _client = client;
            _db = db;
            _img = img;

            _client.UserJoined += WelcomeUser;
        }

        private Task WelcomeUser(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (user.IsBot) return;
                if (OnCooldown(user)) return;
                var cfg = await _db.GetOrCreateWelcomeConfigAsync(user.Guild);
                if (!cfg.Channel.HasValue) return;
                if (IsRatelimited(user, cfg)) return;
                var msg = CreateMessage(cfg.Message, user, user.Guild);
                IMessage message;
                if (cfg.Banner)
                {
                    var banner = await _img.Builder(user);
                    banner.Seek(0, SeekOrigin.Begin);
                    message = await user.Guild.GetTextChannel(cfg.Channel.Value)
                        .SendFileAsync(banner, "Welcome.png", msg);
                }
                else
                {
                    message = await user.Guild.GetTextChannel(cfg.Channel.Value).SendMessageAsync(msg);
                }
                if (cfg.TimeToDelete.HasValue)
                {
                    await Task.Delay(cfg.TimeToDelete.Value);
                    await message.DeleteAsync();
                }
            });
            return Task.CompletedTask;
        }

        private bool OnCooldown(SocketGuildUser user)
        {
            var users = _cooldown.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return true;
            users.Set(user.Id, user, TimeSpan.FromSeconds(60));
            return false;
        }
        
        private bool IsRatelimited(SocketGuildUser user, WelcomeConfig cfg)
        {
            var users = _ratelimit.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.Count + 1 >= cfg.Limit) return true;
            users.Set(user.Id, user.Id, TimeSpan.FromSeconds(5));
            return false;
        }
    }
}