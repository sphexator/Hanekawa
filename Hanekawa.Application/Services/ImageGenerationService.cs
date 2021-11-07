using System.Threading.Tasks;
using Hanekawa.Interfaces.Services;

namespace Hanekawa.Application.Services
{
    public class ImageGenerationService : IImageGenerationService
    {
        public ValueTask<byte[]> CreateAvatarAsync(string url, int width, int height, int radius = 0)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<byte[]> CreateWelcomeImageAsync(string url, int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<byte[]> CreateProfileImageAsync(string url, int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<byte[]> CreateHungerGameImageAsync(string url, int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<byte[]> CreateGameImageAsync(string url, int width, int height)
        {
            throw new System.NotImplementedException();
        }
    }
}