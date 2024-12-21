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
public class ImageService : IImageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FontCollection _fontCollection;
    private readonly IOptionsMonitor<ImageSettings> _settings;
    private readonly ILogger<ImageService> _logger;
    private readonly IDbContext _dbContext;
    private readonly IConfigService _configService;

    public ImageService(IHttpClientFactory httpClientFactory, FontCollection fontCollection,
        IOptionsMonitor<ImageSettings> settings, ILogger<ImageService> logger, IDbContext dbContext, IConfigService configService)
    {
        _httpClientFactory = httpClientFactory;
        _fontCollection = fontCollection;
        _settings = settings;
        _logger = logger;
        _dbContext = dbContext;
        _configService = configService;
    }

    /// <inheritdoc />
    public Task<Stream> DrawWelcomeAsync(DiscordMember member, GreetConfig cfg,
        CancellationToken cancellationToken = default)
    {
        return new WelcomeImageService(_httpClientFactory, _fontCollection, _logger)
            .DrawAsync(member, cfg, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream> DrawProfileAsync(DiscordMember member, GuildUser userData,
        CancellationToken cancellationToken = default)
    {
        return new ProfileImageService(_settings.CurrentValue, _httpClientFactory,
                _dbContext, _fontCollection, _logger, _configService)
            .DrawAsync(member, userData, cancellationToken);
    }
}