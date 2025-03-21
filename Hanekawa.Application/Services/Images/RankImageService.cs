using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Application.Services.Images;

internal class RankImageService
{
    private readonly CommonImageService _common;
    private readonly ImageSettings currentValue;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IDbContext dbContext;
    private readonly FontCollection fontCollection;
    private readonly ILogger<ImageService> logger;
    private readonly IConfigService configService;

    public RankImageService(ImageSettings currentValue, IHttpClientFactory httpClientFactory, IDbContext dbContext, 
    FontCollection fontCollection, ILogger<ImageService> logger, IConfigService configService)
    {
        _common = new CommonImageService(httpClientFactory);
        this.currentValue = currentValue;
        this.httpClientFactory = httpClientFactory;
        this.dbContext = dbContext;
        this.fontCollection = fontCollection;
        this.logger = logger;
        this.configService = configService;
    }
    
    internal async Task<Stream> DrawAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken)
    {
        var image = new Image<Rgba32>(currentValue.Rank.Width, currentValue.Rank.Height);
        image.Mutate(x => x.Fill(Color.White));

        var avatar = await _common.CreateAvatarAsync(member.AvatarUrl, currentValue.Rank.Avatar.Size, cancellationToken);
        image.Mutate(x => x.DrawImage(avatar, new Point(currentValue.Rank.Avatar.X, currentValue.Rank.Avatar.Y), 1f));

        var font = this.fontCollection.Get(currentValue.Rank.Font);

        for (int i = 0; i < currentValue.Rank.Texts.Length; i++)
        {
            TextSettings? text = currentValue.Rank.Texts[i];
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

            var textFont = text.Headline 
                ? new Font(font, text.Size, FontStyle.Bold) 
                : new Font(font, text.Size);

            image.Mutate(x => x.DrawText(
                textValue,
                textFont,
                Color.Black,
                new Point(text.TextPosition.X, text.TextPosition.Y)
            ));
        }   

        var stream = new MemoryStream();
        await image.SaveAsync(stream, new WebpEncoder(), cancellationToken);
        stream.Position = 0;
        return stream;
    }
}