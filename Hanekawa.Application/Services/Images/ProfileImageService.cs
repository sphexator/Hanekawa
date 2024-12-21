using System.Numerics;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = SixLabors.ImageSharp.Drawing.Path;

namespace Hanekawa.Application.Services.Images;

public class ProfileImageService
{
    private readonly IDbContext _dbContext;
    private readonly IConfigService _config;
    private readonly FontCollection _fontCollection;
    private readonly ImageSettings _settings;
    private readonly CommonImageService _common;
    private readonly ILogger _logger;

    public ProfileImageService(ImageSettings settings, IHttpClientFactory httpClientFactory,
        IDbContext dbContext, FontCollection fontCollection, ILogger logger, IConfigService config)
    {
        _common = new CommonImageService(httpClientFactory);
        _dbContext = dbContext;
        _fontCollection = fontCollection;
        _logger = logger;
        _config = config;
        _settings = settings;
    }

    private static Image<Rgba64> ProfileTemplate => Image.Load<Rgba64>(ProfileTemplatePath);
    private static string ProfileTemplatePath => $"{Directory.GetCurrentDirectory()}/Data/Template/ProfileTemplate.png";

    public async Task<Stream> DrawAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        using var img = new Image<Rgba64>(_settings.Profile.Width, _settings.Profile.Height);
        var avatar = await _common.CreateAvatarAsync(member.AvatarUrl, _settings.Profile.Avatar.Size, cancellationToken);
        img.Mutate(async void (x) =>
        {
            try
            {
                x.DrawImage(ProfileTemplate, new Point(0, 0), new GraphicsOptions()); // image
                x.DrawImage(avatar,
                    new Point(_settings.Profile.Avatar.X, _settings.Profile.Avatar.Y),
                    new GraphicsOptions { Antialias = true });

                await ApplyText(x, _settings.Profile.Texts, member, userData, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Async void error drawing profile image");
            }
        });
        var progressBar = CreateProgressBar(userData, avatar.Height);
        img.Mutate(x =>
            x.DrawLine(new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    BlendPercentage = 1
                }
            }, Color.Gray, 3, CreateProgressBar(1, avatar.Height)));
        if (progressBar.Length > 0)
        {
            img.Mutate(x => x.DrawLine(Color.Red,3, progressBar));
        }

        await img.SaveAsync(toReturn, WebpFormat.Instance, cancellationToken: cancellationToken);
        return toReturn;
    }

    private async Task ApplyText(IImageProcessingContext x, TextSettings[] texts, DiscordMember member,
        GuildUser userData,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < texts.Length; i++)
        {
            var item = texts[i];
            var points = item.Position.Select(e => new PointF(e.X, e.Y)).ToArray();
            try
            {
                if (!item.Headline)
                {
                    var textValue = await HandleCustomTextAsync(member, userData, item, cancellationToken);
                    DrawTextValue(x, textValue, item, points);
                }

                var value = "undefined";
                switch (item.SourceType)
                {
                    case nameof(GuildUser):
                        value = userData.GetType().GetProperty(item.SourceField)?.GetValue(userData)?.ToString()
                                ?? "undefined";
                        break;
                    case nameof(DiscordMember):
                        value = member.GetType().GetProperty(item.SourceField)?.GetValue(member)?.ToString()
                                ?? "undefined";
                        break;
                    case "Custom":
                        value = await HandleCustomSourceTextAsync(member, userData, item, cancellationToken);
                        break;
                }

                DrawTextValue(x, value, item, points, HorizontalAlignment.Right);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error drawing text {TextName} on profile image", item.Text);
            }
        }
    }

    private async Task<string> HandleCustomTextAsync(DiscordMember member, GuildUser userData, TextSettings item,
        CancellationToken cancellationToken = default)
    {
        var value = "";
        switch (item.TextType)
        {
            case "Regular":
                value = item.Text;
                break;
            case "Custom":
                switch (item.Text)
                {
                    case "Currency":
                        var config = await _config.GetAsync(userData.GuildId,
                            typeof(CurrencyConfig), cancellationToken);
                        if (config is { CurrencyConfig: null })
                        {
                            config.CurrencyConfig = new CurrencyConfig();
                        }
                        value = config.CurrencyConfig.CurrencyName;
                        break;
                }
                break;
        }
        return value;
    }

    private async Task<string> HandleCustomSourceTextAsync(DiscordMember member, GuildUser userData, TextSettings item,
        CancellationToken cancellationToken = default)
    {
        var value = "";
        switch (item.SourceField)
        {
            case "ServerRank":
                var rank = await _dbContext.Users.CountAsync(e => e.GuildId == member.GuildId
                                                                  && e.Experience >= userData.Experience,
                    cancellationToken);
                var count = await _dbContext.Users.CountAsync(e => e.GuildId == member.GuildId,
                    cancellationToken);
                value = $"{rank.Humanize()}/{count.Humanize()}";
                break;
        }
        return value;
    }

    private static PointF[] CreateProgressBar(float percentage, int size)
    {
        var numb = percentage * 100 / 100 * 360 * 2;
        var points = new PointF[Convert.ToInt32(numb)];
        //const double radius = 55;
        var radius = (int) Math.Ceiling((size * Math.PI) / (2 * Math.PI));
        for (var i = 0; i < numb; i++)
        {
            var radians = i * Math.PI / 360;

            var x = 200 + radius * Math.Cos(radians - Math.PI / 2);
            var y = 58 + radius * Math.Sin(radians - Math.PI / 2);
            points[i] = (new PointF((float) x, (float) y));
        }
        return points;
    }

    private static PointF[] CreateProgressBar(GuildUser userData, int avatarSize)
    {
        var exp = userData.CurrentLevelExperience;
        if (exp == 0) exp = 1;
        var percentage = (exp) / (float) userData.NextLevelExperience;

        return CreateProgressBar(percentage, avatarSize);
    }

    private void DrawTextValue(IImageProcessingContext context, string text, TextSettings item, PointF[] points, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var position = Vector2.Zero;
        if (item.Headline)
        {
            alignment = HorizontalAlignment.Center;
        }

        switch (alignment)
        {
            case HorizontalAlignment.Left:
                position.X = 0;
                break;
            case HorizontalAlignment.Right:
                position.X = points[1].X;
                break;
            case HorizontalAlignment.Center:
                position.X = points[1].X / 2F;
                break;
        }

        var font = new Font(_fontCollection.Get(_settings.Profile.Font), item.Size);
        var options = new RichTextOptions(font)
        {
            Path = new Path(new LinearLineSegment(points)),
            HorizontalAlignment = alignment,
            Origin = position
        };
        context.DrawText(options, text, Color.White);
    }
}