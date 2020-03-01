using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService
    {
        public Task PlayAsync(SocketTextChannel channel, SocketVoiceChannel vc, string url,
            params SocketGuildUser[] users) => Task.CompletedTask;
    }
}