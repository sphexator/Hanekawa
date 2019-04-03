using System.Linq;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public AchievementService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            _client.MessageReceived += MessageCount;
        }

        private Task MessageCount(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                if (msg.Content.Length != 1499) return;

                var achievements = await _db.Achievements.Where(x => x.TypeId == Fun).ToListAsync();
                if (achievements == null) return;

                if (achievements.Any(x => x.Requirement == msg.Content.Length && x.Once))
                {
                    var achieve = achievements.FirstOrDefault(x => x.Requirement == msg.Content.Length && x.Once);
                    if (achieve != null)
                    {
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Fun,
                            UserId = user.Id,
                            Achievement = achieve
                        };
                        await _db.AchievementUnlocks.AddAsync(data);
                        await _db.SaveChangesAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}