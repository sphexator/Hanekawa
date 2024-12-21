using Hanekawa.Application.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Application.Services.Images;

public class CommonImageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CommonImageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Draws an avatar onto the image
    /// </summary>
    /// <param name="avatarUrl"></param>
    /// <param name="size">Size of avatar</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Image> CreateAvatarAsync(string avatarUrl, int size, CancellationToken cancellationToken = default)
    {
        using var img = await GetImageFromUrlAsync(new Uri(avatarUrl), cancellationToken);
        img.Mutate(e =>
            e.ConvertToAvatar(new Size(size), (int)Math.Ceiling((size * Math.PI) / (2 * Math.PI))));
        return img.CloneAs<Rgba64>();
    }

    /// <summary>
    /// Obtains the image memory stream from the given URI.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<Image> GetImageFromUrlAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ImageService");
        var imgStream = await client.GetStreamAsync(uri, cancellationToken);
        return await Image.LoadAsync<Rgba64>(imgStream, cancellationToken);
    }
}