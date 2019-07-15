using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService
    {
        public Task PlayAsync(SocketTextChannel channel, SocketVoiceChannel vc, string url, params SocketGuildUser[] users)
        {
            // todo: Karaoke
            // Add async later, this is to avoid the mark
            // Param users is for duet or multiple people having a karaoke session
            return Task.CompletedTask;
        }
    }
}
