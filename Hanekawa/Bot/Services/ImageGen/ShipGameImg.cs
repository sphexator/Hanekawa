using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using SixLabors.ImageSharp;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> ShipGameBuilder(SocketGuildUser playerOne, SocketGuildUser playerTwo)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load("Data/Game/Background.png"))
            {

            }

            return stream;
        }
    }
}
