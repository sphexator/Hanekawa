using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IImageGenerationService
    {
        ValueTask<byte[]> CreateAvatarAsync(string url, int width, int height, int radius = 0);
        ValueTask<byte[]> CreateWelcomeImageAsync(string url, int width, int height);
        ValueTask<byte[]> CreateProfileImageAsync(string url, int width, int height);
        ValueTask<byte[]> CreateHungerGameImageAsync(string url, int width, int height);
        ValueTask<byte[]> CreateGameImageAsync(string url, int width, int height);
    }
}