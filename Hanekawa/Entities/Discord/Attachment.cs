using System.IO;

namespace Hanekawa.Entities.Discord;

public class Attachment
{
    public MemoryStream Stream { get; set; }
    public string FileName { get; set; }
}