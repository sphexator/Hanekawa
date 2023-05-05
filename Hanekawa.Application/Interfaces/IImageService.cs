using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces;

public interface IImageService
{
    /// <summary>
    /// Draws an avatar onto the image image
    /// </summary>
    /// <param name="image">Image stream to draw avatar to</param>
    /// <param name="size">Size of avatar</param>
    /// <param name="xPos">X position on the image to start drawing</param>
    /// <param name="yPos">Y position on the image to start drawing</param>
    /// <param name="isCircle">Whether the avatar should be a circle, or square. Default is square.</param>
    /// <returns></returns>
    Task<Stream> DrawAvatarAsync(Stream image, int size, int xPos, int yPos, bool isCircle = false);
    /// <summary>
    /// Creates a welcome image
    /// </summary>
    /// <param name="member">Discord user</param>
    /// <returns></returns>
    Task<Stream> DrawWelcome(DiscordMember member);
    /// <summary>
    /// Creates a profile image
    /// </summary>
    /// <param name="member">Discord user</param>
    /// <returns></returns>
    Task<Stream> DrawProfile(DiscordMember member);
}