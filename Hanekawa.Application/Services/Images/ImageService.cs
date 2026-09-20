using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;

namespace Hanekawa.Application.Services.Images;

/// <inheritdoc />
public class ImageService(IHttpClientFactory httpClientFactory, 
	FontCollection fontCollection,
	IOptionsMonitor<ImageSettings> settings, 
	ILogger<ImageService> logger, 
	IDbContext dbContext, 
	IConfigService configService) : IImageService
{

	/// <inheritdoc />
    public ValueTask<Stream> DrawWelcomeAsync(DiscordMember member, GreetConfig cfg,
        CancellationToken cancellationToken = default)
    {
        return new WelcomeImageService(httpClientFactory, fontCollection, logger)
            .DrawAsync(member, cfg, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<Stream> DrawProfileAsync(DiscordMember member, GuildUser userData,
        CancellationToken cancellationToken = default)
    {
        return new ProfileImageService(settings.CurrentValue, httpClientFactory,
                dbContext, fontCollection, logger, configService)
            .DrawAsync(member, userData, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream> DrawRankAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default)
    {
        return new RankImageService(settings.CurrentValue, httpClientFactory,
                dbContext, fontCollection, logger, configService)
            .DrawAsync(member, userData, cancellationToken);
    }
}