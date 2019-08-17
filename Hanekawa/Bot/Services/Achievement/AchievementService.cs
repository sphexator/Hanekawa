using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
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
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;

        public AchievementService(DiscordSocketClient client, InternalLogService log)
        {
            _client = client;
            _log = log;

            _client.MessageReceived += MessageCount;
        }

        private Task MessageCount(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                if (msg.Content.Length != 1499) return;
                try
                {
                    using (var db = new DbService())
                    {
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
                                    UserId = user.Id,
                                    Achievement = achieve
                                };
                                await db.AchievementUnlocks.AddAsync(data);
                                await db.SaveChangesAsync();
                                _log.LogAction(LogLevel.Information, null, $"(Achievement Service) {user.Id} scored {achieve.Name} in {user.Guild.Id}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Achievement Service) Error for {user.Id} in {user.Guild.Id} for Message Count - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}