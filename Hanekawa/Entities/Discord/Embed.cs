using System;

namespace Hanekawa.Entities.Discord;

/// <summary>
/// Discord embed
/// </summary>
public class Embed
{
    /// <summary>
    /// Header tuple, first string is avatar, other is header test
    /// </summary>
    public Tuple<string, string> Header { get; set; }
    /// <summary>
    /// Title of embed
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Major content of embed
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// Color of embed
    /// </summary>
    public int Color { get; set; }
    /// <summary>
    /// Timestamp put with the footer if provided
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    /// <summary>
    /// Icon of the embed
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// Attachment of the embed
    /// </summary>
    public string Attachment { get; set; }
    /// <summary>
    /// Footer tuple, first string is avatar if provided, other is footer text that's next to timestamp.
    /// </summary>
    public Tuple<string, string> Footer { get; set; }
}