using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using SixLabors.ImageSharp;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public Task<Stream> ShipGameBuilder(SocketGuildUser playerOne, SocketGuildUser playerTwo)
        {
            // TODO: Ship game builder
            var stream = new MemoryStream();
            using (var img = Image.Load("Data/Game/Background.png"))
            {
            }

            return Task.FromResult<Stream>(stream);
        }
    }
}