namespace Hanekawa.Entities;

/// <summary>
/// Represents a simple message response
/// </summary>
public class Message
{
    /// <summary>
    /// The content of the message
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Creates a new message with the specified content
    /// </summary>
    /// <param name="content">The message content</param>
    public Message(string content)
    {
        Content = content;
    }
}
