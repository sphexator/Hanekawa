using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using Hanekawa.Extensions;
using Humanizer;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services.Images;

public class ProfileImageService(ImageSettings settings, 
	IHttpClientFactory httpClientFactory,
	IDbContext dbContext, 
	FontCollection fontCollection, 
	ILogger logger, 
	IConfigService config)
{
	private readonly CommonImageService _common = new(httpClientFactory);

	private static IMagickImage ProfileTemplate => new MagickImage(ProfileTemplatePath);
    private static string ProfileTemplatePath => $"{Directory.GetCurrentDirectory()}/Data/Template/ProfileTemplate.png";

    public async ValueTask<Stream> DrawAsync(DiscordMember member, GuildUser userData, CancellationToken cancellationToken = default)
    {
        var toReturn = new MemoryStream();
        using var img = new MagickImage(MagickColors.Transparent, (uint)settings.Profile.Width, (uint)settings.Profile.Height);
        using var avatar = await _common.CreateAvatarAsync(member.AvatarUrl, settings.Profile.Avatar.Size, cancellationToken);
        using (var template = ProfileTemplate)
        {
            img.Composite(template, 0, 0, CompositeOperator.Over);
        }
        img.Composite(avatar, settings.Profile.Avatar.X, settings.Profile.Avatar.Y, CompositeOperator.Atop);

        await ApplyText(img, settings.Profile.Texts, member, userData, cancellationToken);
        var progressBar = CreateProgressBar(userData, (int)avatar.Height);
        new Drawables()
            .StrokeColor(MagickColors.Gray)
            .StrokeWidth(3)
            .FillOpacity(new Percentage(0))
            .Polyline(CreateProgressBar(1, (int)avatar.Height))
            .Draw(img);
        if (progressBar.Length > 0)
        {
            new Drawables()
                .StrokeColor(MagickColors.Red)
                .StrokeWidth(3)
                .FillOpacity(new Percentage(0))
                .Polyline(progressBar)
                .Draw(img);
        }

        await img.WriteAsync(toReturn, MagickFormat.WebP, cancellationToken);
        toReturn.Position = 0;
        return toReturn;
    }

    private async ValueTask ApplyText(IMagickImage<ushort> img, TextSettings[] texts, DiscordMember member,
        GuildUser userData,
        CancellationToken cancellationToken)
    {
        var draws = new List<(string Text, TextSettings Item, PointD[] Points, TextAlignment Alignment)>();
        for (var i = 0; i < texts.Length; i++)
        {
            var item = texts[i];
            var points = item.Position.Select(e => new PointD(e.X, e.Y)).ToArray();
            try
            {
                if (!item.Headline)
                {
                    var textValue = await HandleCustomTextAsync(member, userData, item, cancellationToken);
                    draws.Add((textValue, item, points, TextAlignment.Left));
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

                draws.Add((value, item, points, TextAlignment.Right));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error drawing text {TextName} on profile image", item.Text);
            }
        }

        foreach (var (text, item, points, alignment) in draws)
        {
            DrawTextValue(img, text, item, points, alignment);
        }
    }

    private async ValueTask<string> HandleCustomTextAsync(DiscordMember member, GuildUser userData, TextSettings item,
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
                        var config1 = await config.GetAsync(userData.GuildId,
                            typeof(CurrencyConfig), cancellationToken);
                        if (config1 is { CurrencyConfig: null })
                        {
                            config1.CurrencyConfig = new CurrencyConfig();
                        }
                        value = config1.CurrencyConfig.CurrencyName;
                        break;
                }
                break;
        }
        return value;
    }

    private async ValueTask<string> HandleCustomSourceTextAsync(DiscordMember member, GuildUser userData, TextSettings item,
        CancellationToken cancellationToken = default)
    {
        var value = "";
        switch (item.SourceField)
        {
            case "ServerRank":
                var rank = await dbContext.Users.CountAsync(e => e.GuildId == member.GuildId
                                                                  && e.Experience >= userData.Experience,
                    cancellationToken);
                var count = await dbContext.Users.CountAsync(e => e.GuildId == member.GuildId,
                    cancellationToken);
                value = $"{rank.Humanize()}/{count.Humanize()}";
                break;
        }
        return value;
    }

    private static PointD[] CreateProgressBar(float percentage, int size)
    {
        var numb = percentage * 100 / 100 * 360 * 2;
        var points = new PointD[Convert.ToInt32(numb)];
        //const double radius = 55;
        var radius = (int) Math.Ceiling((size * Math.PI) / (2 * Math.PI));
        
        for (var i = 0; i < points.Length; i++)
        {
            var radians = i * Math.PI / 360;

            var x = 200 + radius * Math.Cos(radians - Math.PI / 2);
            var y = 58 + radius * Math.Sin(radians - Math.PI / 2);
            points[i] = new PointD(x, y);
        }
        return points;
    }

    private static PointD[] CreateProgressBar(GuildUser userData, int avatarSize)
    {
        var exp = userData.CurrentLevelExperience;
        if (exp == 0) exp = 1;
        var percentage = (exp) / (float) userData.NextLevelExperience;

        return CreateProgressBar(percentage, avatarSize);
    }

    private void DrawTextValue(IMagickImage<ushort> img, string text, TextSettings item, PointD[] points, TextAlignment alignment = TextAlignment.Left)
    {
        if (item.Headline)
        {
            alignment = TextAlignment.Center;
        }

        double x;
        switch (alignment)
        {
            case TextAlignment.Right:
                x = points[1].X;
                break;
            case TextAlignment.Center:
                x = (points[0].X + points[1].X) / 2.0;
                break;
            default:
                x = points[0].X;
                break;
        }

        var y = points[0].Y + item.Size;
        new Drawables()
            .Font(fontCollection.Get(settings.Profile.Font))
            .FontPointSize(item.Size)
            .FillColor(MagickColors.White)
            .TextAlignment(alignment)
            .Text(x, y, text)
            .Draw(img);
    }
}