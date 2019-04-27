using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService
    {
        public async Task PlayAsync(SocketGuildUser user, SocketTextChannel channel, SocketVoiceChannel vc, string url)
        {

        }

        public async Task PlayAsync(SocketTextChannel channel, SocketVoiceChannel vc, string url, params SocketGuildUser[] users)
        {

        }

        public async Task SkipAsync(SocketGuild guild)
        {

        }

        public async Task SetVolumeAsync(SocketGuild guild, int volume)
        {

        }

        public async Task SetPositionAsync(SocketGuildUser user, TimeSpan position)
        {

        }
    }
}
