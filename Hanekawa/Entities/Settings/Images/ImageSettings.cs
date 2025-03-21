using System.Text.Json.Serialization;

namespace Hanekawa.Entities.Settings.Images;

public class ImageSettings
{
    [JsonPropertyName("Profile")]
    public ProfileSettings Profile { get; set; } = new();
    public RankSettings Rank { get; set; } = new();
}

public class RankSettings : ImageSize
{
    [JsonPropertyName("Avatar")]
    public AvatarSettings Avatar { get; set; } = new();

    [JsonPropertyName("Font")]
    public string Font { get; set; } = string.Empty;

    [JsonPropertyName("Texts")]
    public TextSettings[] Texts { get; set; } = [];
}

public class ProfileSettings : ImageSize
{
    [JsonPropertyName("Avatar")]
    public AvatarSettings Avatar { get; set; } = new();

    [JsonPropertyName("Font")]
    public string Font { get; set; } = string.Empty;

    [JsonPropertyName("Texts")]
    public TextSettings[] Texts { get; set; } = [];
}

public class AvatarSettings : ImagePosition
{
    [JsonPropertyName("Size")]
    public int Size { get; set; } = 0;
}

public class TextSettings
{
    [JsonPropertyName("TextType")]
    public string TextType { get; set; } = string.Empty;

    [JsonPropertyName("Text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("TextPosition")]
    public ImagePosition TextPosition { get; set; } = new();

    [JsonPropertyName("Headline")]
    public bool Headline { get; set; } = false;

    [JsonPropertyName("SourceType")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("SourceField")]
    public string SourceField { get; set; } = string.Empty;

    [JsonPropertyName("Position")]
    public ImagePosition[] Position { get; set; } = [];

    [JsonPropertyName("Size")]
    public int Size { get; set; } = 0;
}

public abstract class ImageSize
{
    /// <summary>
    /// Gets or sets the width of the image.
    /// </summary>
    [JsonPropertyName("Width")]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the image.
    /// </summary>
    [JsonPropertyName("Height")]
    public int Height { get; set; }
}

public class ImagePosition
{
    /// <summary>
    /// Gets or sets the X coordinate of the image.
    /// </summary>
    [JsonPropertyName("X")]
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the image.
    /// </summary>
    [JsonPropertyName("Y")]
    public int Y { get; set; }
}