using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services.Images;

public class WelcomeImageService
{
    private readonly CommonImageService _common;
    private readonly FontCollection _fontCollection;
    private ILogger _logger;

    public WelcomeImageService(IHttpClientFactory httpClientFactory, FontCollection fontCollection, ILogger logger)
    {
        _common = new CommonImageService(httpClientFactory);
        _fontCollection = fontCollection;
        _logger = logger;
    }

    public async ValueTask<Stream> DrawAsync(DiscordMember member, GreetConfig cfg, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        var dbImage = cfg.Images[Random.Shared.Next(cfg.Images.Count - 1)];
        using var image = await _common.GetImageFromUrlAsync(new Uri(dbImage.ImageUrl), cancellationToken);

        using var avatar =
            await _common.CreateAvatarAsync(member.AvatarUrl, dbImage.AvatarSize, cancellationToken);
        image.Composite(avatar, dbImage.AvatarX, dbImage.AvatarY, CompositeOperator.Over);
        new Drawables()
            .Font(_fontCollection.Get("Arial"))
            .FontPointSize(dbImage.UsernameSize)
            .FillColor(MagickColors.White)
            .Text(dbImage.UsernameX, dbImage.UsernameY + dbImage.UsernameSize, member.Username)
            .Draw(image);

        await image.WriteAsync(toReturn, MagickFormat.Png, cancellationToken);
        toReturn.Position = 0;
        return toReturn;
    }
}