using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

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

    public async Task<Stream> DrawAsync(DiscordMember member, GreetConfig cfg, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        var dbImage = cfg.Images[Random.Shared.Next(cfg.Images.Count - 1)];
        var image = await _common.GetImageFromUrlAsync(new Uri(dbImage.ImageUrl), cancellationToken);

        var avatar =
            await _common.CreateAvatarAsync(member.AvatarUrl, dbImage.AvatarSize, cancellationToken);
        image.Mutate(x =>
        {
            x.DrawImage(avatar, new Point(dbImage.AvatarX, dbImage.AvatarY), 1f);
            x.DrawText(member.Username, new Font(_fontCollection.Get("Arial"), dbImage.UsernameSize), Color.White,
                new Point(dbImage.UsernameX, dbImage.UsernameY));
        });

        await image.SaveAsync(toReturn, PngFormat.Instance, cancellationToken: cancellationToken);
        return toReturn;
    }
}