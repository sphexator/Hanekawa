using System;
using System.Collections.Generic;

namespace Hanekawa.Entities.Discord;

/// <summary>
/// Discord embed
/// </summary>
public class Embed
{
    /// <summary>
    /// Header tuple, First value = Avatar Url, Second value = Content value, Third value = Uri
    /// </summary>
    public Tuple<string, string, string> Header { get; set; }
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
    /// Fields of tuple. First value = title, second value = content
    /// </summary>
    public List<Tuple<string, string, bool>> Fields { get; set; }
    /// <summary>
    /// Icon of the embed
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// Attachment of the embed
    /// </summary>
    public string Attachment { get; set; }
    /// <summary>
    /// Footer tuple.First value = Avatar url. Second value = text
    /// </summary>
    public Tuple<string, string> Footer { get; set; }
}