using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using SixLabors.ImageSharp;

namespace Hanekawa.Application.Interfaces;

/// <summary>
/// Service for creating images
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Creates a rank image
    /// </summary>
    /// <param name="member">Discord user</param>
    /// <param name="userData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DrawRankAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default);

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