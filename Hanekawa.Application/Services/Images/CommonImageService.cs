using Hanekawa.Application.Extensions;
using ImageMagick;

namespace Hanekawa.Application.Services.Images;

public class CommonImageService(IHttpClientFactory httpClientFactory)
{
	/// <summary>
    /// Draws an avatar onto the image
    /// </summary>
    /// <param name="avatarUrl"></param>
    /// <param name="size">Size of avatar</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IMagickImage<ushort>> CreateAvatarAsync(string avatarUrl, int size, CancellationToken cancellationToken = default)
    {
        var img = await GetImageFromUrlAsync(new Uri(avatarUrl), cancellationToken);
        img.ConvertToAvatar(size, (int)Math.Ceiling((size * Math.PI) / (2 * Math.PI)));
        return img;
    }

    /// <summary>
    /// Gets the image memory stream from the given URI.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<IMagickImage<ushort>> GetImageFromUrlAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("ImageService");
        var bytes = await client.GetByteArrayAsync(uri, cancellationToken);
        return new MagickImage(bytes);
    }
}