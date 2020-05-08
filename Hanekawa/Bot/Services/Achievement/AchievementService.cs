using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Achievement;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService : INService, IRequired
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;

        public AchievementService(DiscordBot client, InternalLogService log, IServiceProvider provider)
        {
            _client = client;
            _log = log;
            _provider = provider;

            _client.MessageReceived += MessageCount;
        }

        private Task MessageCount(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var msg = e.Message;
                if (!(msg.Author is CachedMember user)) return;
                if (user.IsBot) return;
                if (msg.Content.Length != 1499) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var achievements = await db.Achievements.Where(x => x.TypeId == Fun).ToListAsync();
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == msg.Content.Length && x.Once))
                    {
                        var achieve =
                            achievements.FirstOrDefault(x => x.Requirement == msg.Content.Length && x.Once);
                        if (achieve != null)
                        {
                            var data = new AchievementUnlock
                            {
                                AchievementId = achieve.AchievementId,
                                TypeId = Fun,
                                UserId = user.Id.RawValue,
                                Achievement = achieve
                            };
                            await db.AchievementUnlocks.AddAsync(data);
                            await db.SaveChangesAsync();
                            _log.LogAction(LogLevel.Information, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.Guild.Id.RawValue}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Achievement Service) Error for {user.Id.RawValue} in {user.Guild.Id.RawValue} for Message Count - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}