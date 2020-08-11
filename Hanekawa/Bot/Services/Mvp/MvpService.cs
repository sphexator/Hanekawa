using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Hanekawa.Bot.Services.Mvp
{
    public class MvpService : INService, IRequired, IJob
    {
        private readonly DiscordClient _client;
        private InternalLogService _log;
        private readonly IServiceProvider _service;
        private static readonly ConcurrentDictionary<ulong, MemoryCache> Cooldown = new ConcurrentDictionary<ulong, MemoryCache>();
        private static readonly List<ulong> Premium = new List<ulong>();

        public MvpService(DiscordClient client, InternalLogService log, IServiceProvider service)
        {
            _client = client;
            _log = log;
            _service = service;

            _client.MessageReceived += CountMvp;
        }

        private Task CountMvp(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Message.Author.IsBot) return;
                if (!(e.Message.Author is CachedMember user)) return;
                if (!(await ServerCheck(user.Guild))) return;
                var userCd = Cooldown.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
                if (userCd.TryGetValue(user.Id.RawValue, out _)) return;
                userCd.CreateEntry(user.Id.RawValue);
                using var scope = _service.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var userData = await db.GetOrCreateUserData(user);
                userData.MvpCount++;
                db.Accounts.Update(userData);
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private async Task<bool> ServerCheck(CachedGuild guild)
        {
            if (Premium.Contains(guild.Id.RawValue)) return true;
            using (var scope = _service.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
            {
                var guildCfg = await db.GetOrCreateGuildConfigAsync(guild);
                if (!guildCfg.Premium) return false;
                Premium.Add(guild.Id.RawValue);
            }
            return true;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = MvpReward();
            return Task.CompletedTask;
        }

        private async Task MvpReward()
        {
            using var scope = _service.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();

        }
    }
}
