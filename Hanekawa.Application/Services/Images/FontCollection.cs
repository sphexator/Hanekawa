namespace Hanekawa.Application.Services.Images;

/// <summary>
/// Registry that maps font names to font files for Magick.NET rendering.
/// </summary>
public class FontCollection
{
    private readonly Dictionary<string, string> _fonts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registered font names.
    /// </summary>
    public IReadOnlyCollection<string> Names => _fonts.Keys.ToArray();

    /// <summary>
    /// Registers a font file. The file name without extension becomes the font name.
    /// </summary>
    public void Add(string path) => _fonts[Path.GetFileNameWithoutExtension(path)] = path;

    /// <summary>
    /// Registers all fonts found in the operating system's font directories.
    /// </summary>
    public void AddSystemFonts()
    {
        foreach (var directory in GetSystemFontDirectories())
        {
            if (!Directory.Exists(directory)) continue;
            foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
                    || file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                {
                    _fonts.TryAdd(Path.GetFileNameWithoutExtension(file), file);
                }
            }
        }
    }

    /// <summary>
    /// Resolves a font name to a font file path. Falls back to the first registered font.
    /// </summary>
    public string Get(string name)
    {
        if (File.Exists(name)) return name;
        if (_fonts.TryGetValue(name, out var path)) return path;
        return _fonts.Values.First();
    }

    private static IEnumerable<string> GetSystemFontDirectories()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return "/System/Library/Fonts";
            yield return "/Library/Fonts";
        }
        else
        {
            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts");
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "fonts");
        }
    }
}
