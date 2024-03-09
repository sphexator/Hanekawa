using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using SixLabors.ImageSharp;

namespace Hanekawa.Application.Interfaces;

public interface IImageService
{
    /// <summary>
    /// Draws an avatar onto the image image
    /// </summary>
    /// <param name="avatarUrl"></param>
    /// <param name="size">Size of avatar</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Image> CreateAvatarAsync(string avatarUrl, int size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a welcome image
    /// </summary>
    /// <param name="member">Discord user</param>
    /// <param name="cfg"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DrawWelcomeAsync(DiscordMember member, GreetConfig cfg, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a profile image
    /// </summary>
    /// <param name="member">Discord user</param>
    /// <param name="userData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DrawProfileAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default);
}