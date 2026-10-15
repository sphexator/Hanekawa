using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services.Images;

internal class RankImageService
{
    private readonly CommonImageService _common;
    private readonly ImageSettings _currentValue;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContext _dbContext;
    private readonly FontCollection _fontCollection;
    private readonly ILogger<ImageService> _logger;
    private readonly IConfigService _configService;

    public RankImageService(ImageSettings currentValue, IHttpClientFactory httpClientFactory, IDbContext dbContext, 
    FontCollection fontCollection, ILogger<ImageService> logger, IConfigService configService)
    {
        _common = new CommonImageService(httpClientFactory);
        _currentValue = currentValue;
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _fontCollection = fontCollection;
        _logger = logger;
        _configService = configService;
    }
    
    internal async Task<Stream> DrawAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken)
    {
        using var image = new MagickImage(MagickColors.White, (uint)_currentValue.Rank.Width, (uint)_currentValue.Rank.Height);

        using var avatar = await _common.CreateAvatarAsync(member.AvatarUrl, _currentValue.Rank.Avatar.Size, cancellationToken);
        image.Composite(avatar, _currentValue.Rank.Avatar.X, _currentValue.Rank.Avatar.Y, CompositeOperator.Over);

        var font = _fontCollection.Get(_currentValue.Rank.Font);

        for (int i = 0; i < _currentValue.Rank.Texts.Length; i++)
        {
            var text = _currentValue.Rank.Texts[i];
            var textValue = text.TextType switch
            {
                "Regular" => text.Text,
                "Custom" => text.SourceType switch
                {
                    "GuildUser" => text.SourceField switch 
                    {
                        "Username" => member.Username,
                        "Level" => userData.Level.ToString(),
                        "Experience" => userData.Experience.ToString(),
                        _ => string.Empty
                    },
                    "ServerRank" => string.Empty,
                    _ => string.Empty
                },
                _ => string.Empty
            };

            var drawables = new Drawables()
                .FontPointSize(text.Size)
                .FillColor(MagickColors.Black);
            drawables = text.Headline
                ? drawables.Font(font, FontStyleType.Any, FontWeight.Bold, FontStretch.Normal)
                : drawables.Font(font);

            drawables
                .Text(text.TextPosition.X, text.TextPosition.Y + text.Size, textValue)
                .Draw(image);
        }   

        var stream = new MemoryStream();
        await image.WriteAsync(stream, MagickFormat.WebP, cancellationToken);
        stream.Position = 0;
        return stream;
    }
}