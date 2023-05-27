using System.Runtime.InteropServices;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class ImageService : IImageService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFontCollection _fontCollection;

    public ImageService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IFontCollection fontCollection)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _fontCollection = fontCollection;
    }

    /// <inheritdoc />
    public async Task<Image> CreateAvatarAsync(string avatarUrl, int size, CancellationToken cancellationToken = default)
    {
        using var img = await GetImageFromUrlAsync(new Uri(avatarUrl), cancellationToken);
        img.Mutate(e => 
            e.ConvertToAvatar(new Size(size), (int)Math.Ceiling((size * Math.PI) / (2 * Math.PI))));
        return img.CloneAs<Rgba32>();
    }
    /// <inheritdoc />
    public async Task<Stream> DrawWelcomeAsync(DiscordMember member, GreetConfig cfg, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        var dbImage = cfg.Images[Random.Shared.Next(cfg.Images.Count)];
        var image = await GetImageFromUrlAsync(new Uri(dbImage.ImageUrl), cancellationToken);

        var avatar =
            await CreateAvatarAsync(member.AvatarUrl, dbImage.AvatarSize, cancellationToken);
        image.Mutate(x =>
        {
            x.DrawImage(avatar, new Point(dbImage.AvatarX, dbImage.AvatarY), 1f);
            x.DrawText(member.Username, new Font(_fontCollection.Get("Arial"), dbImage.UsernameSize), Color.White,
                new Point(dbImage.UsernameX, dbImage.UsernameY));
        });
        
        await image.SaveAsync(toReturn, PngFormat.Instance, cancellationToken: cancellationToken);
        return toReturn;
    }
    /// <inheritdoc />
    public async Task<Stream> DrawProfileAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        using var img = new Image<Rgba32>(400, 400);
        var avatar = await CreateAvatarAsync(member.AvatarUrl, 110, cancellationToken);
        img.Mutate(x =>
        {
            x.DrawImage(Image.Load<Rgba32>("Hanekawa/Data/Template/ProfileTemplate.png"), 
                new Point(0, 0), new GraphicsOptions());
            x.DrawImage(avatar, new Point(145, 4),
                new GraphicsOptions { Antialias = true });
            x.DrawLines(Color.White, 1,CreateProfileProgressBar(userData, 0));
        });
        
        await img.SaveAsync(toReturn, WebpFormat.Instance, cancellationToken: cancellationToken);
        return toReturn;
    }

    /// <summary>
    /// Obtains the image memory stream from the given URI.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Image> GetImageFromUrlAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ImageService");
        var imgStream = await client.GetStreamAsync(uri, cancellationToken);
        return await Image.LoadAsync<Rgba32>(imgStream, cancellationToken);
    }
    
    private PointF[] CreateProfileProgressBar(GuildUser userData, long currentLevelExperience)
    {
        var percentage = (userData.Experience - userData.CurrentLevelExperience)/ (float) userData.NextLevelExperience;
        var numb = percentage * 100 / 100 * 360 * 2;
        var points = new List<PointF>();
        const double radius = 55;

        for (var i = 0; i < numb; i++)
        {
            var radians = i * Math.PI / 360;

            var x = 200 + radius * Math.Cos(radians - Math.PI / 2);
            var y = 59 + radius * Math.Sin(radians - Math.PI / 2);
            points.Add(new PointF((float) x, (float) y));
        }
        return points.ToArray();
    }
}