using System.IO;
using System.Threading.Tasks;
using Disqord;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public Task<Stream> RankBuilder(CachedMember user)
        {
            // TODO: Create rank picture
            var stream = new MemoryStream();

            return Task.FromResult<Stream>(stream);
        }
    }
}