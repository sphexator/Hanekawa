using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Administration
{
    public class PlayStatusService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;

        public PlayStatusService(DiscordSocketClient client)
        {
            _client = client;

            _client.Ready += () => new Task(async () =>
            {
                await _client.SetGameAsync($"'@{_client.CurrentUser.Username} help' for help", "http://www.hanekawabot.moe/", ActivityType.Streaming);
            });
        }
    }
}