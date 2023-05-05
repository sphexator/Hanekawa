using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class ImageService : IImageService
{
    /// <inheritdoc />
    public Task<Stream> DrawAvatarAsync(Stream image, int size, int xPos, int yPos, bool isCircle = false)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc />
    public Task<Stream> DrawWelcome(DiscordMember member)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc />
    public Task<Stream> DrawProfile(DiscordMember member)
    {
        throw new NotImplementedException();
    }
}