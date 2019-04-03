using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using System.Threading.Tasks;

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

            });
            return Task.CompletedTask;
        }
    }
}