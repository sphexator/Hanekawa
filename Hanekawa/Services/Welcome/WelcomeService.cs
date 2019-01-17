using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Services.Welcome
{
    public class WelcomeService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly WelcomeMessage _message;
        private readonly ImageGenerator _image;

        public WelcomeService(DiscordSocketClient discord, WelcomeMessage message, ImageGenerator image)
        {
            _client = discord;
            _message = message;
            _image = image;

            _client.UserJoined += WelcomeMessage;
            _client.UserJoined += WelcomeToggle;
            _client.LeftGuild += BannerCleanup;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                    DisableBanner.AddOrUpdate(x.GuildId, x.WelcomeChannel.HasValue, (arg1, b) => false);
            }
            Console.WriteLine("Welcome service loaded");
        }

        // True = banners enabled
        // False = banners disabled 
        private ConcurrentDictionary<ulong, bool> DisableBanner { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private ConcurrentDictionary<ulong, uint> JoinCount { get; }
            = new ConcurrentDictionary<ulong, uint>();

        private ConcurrentDictionary<ulong, bool> AntiRaidDisable { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private ConcurrentDictionary<ulong, MemoryCache> WelcomeCooldown { get; }
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private Task WelcomeToggle(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                uint counter;
                uint limit;
                using (var db = new DbService())
                {
                    var cfg = await db.GuildConfigs.FindAsync(user.Guild.Id);
                    limit = cfg.WelcomeLimit;
                    counter = JoinCount.GetOrAdd(user.Guild.Id, 0);
                }

                counter++;
                JoinCount.AddOrUpdate(user.Guild.Id, counter, (key, old) => old = counter);

                if (counter >= limit) AntiRaidDisable.AddOrUpdate(user.Guild.Id, true, (key, old) => old = true);
                await Task.Delay(5000);

                JoinCount.TryGetValue(user.Guild.Id, out var currentValue);
                if (currentValue <= limit) AntiRaidDisable.AddOrUpdate(user.Guild.Id, true, (key, old) => old = false);
                JoinCount.AddOrUpdate(user.Guild.Id, currentValue--, (key, old) => old = currentValue--);
            });
            return Task.CompletedTask;
        }

        private Task WelcomeMessage(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (user.IsBot) return;
                if (!CheckCooldown(user)) return;
                if (AntiRaidDisable.GetOrAdd(user.Guild.Id, false)) return;
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild).ConfigureAwait(false);
                    if (!cfg.WelcomeChannel.HasValue) return;

                    var channel = user.Guild.GetTextChannel(cfg.WelcomeChannel.Value);
                    if (channel == null) return;

                    if (cfg.WelcomeBanner) await WelcomeBanner(channel, user, cfg).ConfigureAwait(false);
                    else await channel.SendMessageAsync(_message.Message(cfg.WelcomeMessage, user));
                }
            });
            return Task.CompletedTask;
        }

        private async Task WelcomeBanner(ISocketMessageChannel channel, SocketGuildUser user, GuildConfig cfg)
        {
            var (stream, message) = await _image.Banner(user, cfg.WelcomeMessage).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            var msg = await channel.SendFileAsync(stream, "Welcome.png", message)
                .ConfigureAwait(false);
            if (user.Guild.CurrentUser.GuildPermissions.ManageMessages && cfg.WelcomeDelete.HasValue)
            {
                await Task.Delay(cfg.WelcomeDelete.Value);
                try { await msg.DeleteAsync(); }
                catch { /* IGNORE */ }
            }
        }

        private static Task BannerCleanup(SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                using (var db = new DbService())
                {
                    var banners = db.WelcomeBanners.Where(x => x.GuildId == guild.Id);
                    db.WelcomeBanners.RemoveRange(banners);
                    db.SaveChanges();
                }
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldown(IGuildUser user)
        {
            var users = WelcomeCooldown.GetOrAdd(user.GuildId, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return false;
            users.Set(user.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return true;
        }
    }
}