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
    public EmbedHeader Header { get; set; } = null;

    /// <summary>
    /// Title of embed
    /// </summary>
    public string? Title { get; set; } = null;

    /// <summary>
    /// Major content of embed
    /// </summary>
    public string? Content { get; set; } = null;
    /// <summary>
    /// Color of embed
    /// </summary>
    public int Color { get; set; } = 0;
    /// <summary>
    /// Timestamp put with the footer if provided
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Fields of tuple. First value = title, second value = content
    /// </summary>
    public List<EmbedField> Fields { get; set; } = new();

    /// <summary>
    /// Icon of the embed
    /// </summary>
    public string? Icon { get; set; } = null;

    /// <summary>
    /// Attachment of the embed
    /// </summary>
    public string? Attachment { get; set; } = null;

    /// <summary>
    /// Footer tuple.First value = Avatar url. Second value = text
    /// </summary>
    public EmbedFooter Footer { get; set; } = null;
}