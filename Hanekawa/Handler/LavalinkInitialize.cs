using Discord.WebSocket;
using Hanekawa.Modules.Audio.Service;
using System.Threading.Tasks;
using Victoria;

namespace Hanekawa.Handler
{
    public class LavalinkInitialize
    {
        private readonly DiscordSocketClient _client;
        private readonly Lavalink _lavalink;
        private readonly AudioService _service;

        public LavalinkInitialize(DiscordSocketClient client, Lavalink lavalink, AudioService service)
        {
            _client = client;
            _lavalink = lavalink;
            _service = service;

            _client.Ready += OnReady;
        }

        private async Task OnReady()
        {
            var node = await _lavalink.ConnectAsync(_client);
            _service.Initialize(node);
        }
    }
}
