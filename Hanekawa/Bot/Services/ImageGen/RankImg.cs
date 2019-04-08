using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> RankBuilder(SocketGuildUser user)
        {
            var stream = new MemoryStream();

            return stream;
        }
    }
}