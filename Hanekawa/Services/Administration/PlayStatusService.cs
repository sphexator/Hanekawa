using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;
using System.Threading.Tasks;

namespace Hanekawa.Services.Administration
{
    public class PlayStatusService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;

        public PlayStatusService(DiscordSocketClient client)
        {
            _client = client;

            _client.Ready += ClientOnReady;
        }

        private Task ClientOnReady()
        {
            var _ = Task.Run(async () =>
            {
                await _client.SetGameAsync($"'@{_client.CurrentUser.Username} help' for help");
            });
            return Task.CompletedTask;
        }
    }
}