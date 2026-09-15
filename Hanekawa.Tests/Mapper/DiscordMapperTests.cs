using Disqord;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Tests.Mapper;

public class DiscordMapperTests
{
    [Fact]
    public void ToLocalEmbed_MapsHeaderFooterAndFields()
    {
        var timestamp = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var embed = new Embed
        {
            Title = "Module status",
            Content = "Level is on",
            Color = 0x00FF00,
            Icon = "https://cdn/icon.png",
            Attachment = "https://cdn/banner.png",
            Timestamp = timestamp,
            Header = new EmbedHeader("Hanekawa", "https://cdn/avatar.png", "https://example.com"),
            Footer = new EmbedFooter("https://cdn/footer.png", "Updated"),
            Fields =
            [
                new EmbedField("XP", "1–5 per message", true),
                new EmbedField("Rank", "#3", false)
            ]
        };

        var local = embed.ToLocalEmbed();

        Assert.Equal("Module status", local.Title);
        Assert.Equal("Level is on", local.Description);
        Assert.Equal("https://cdn/icon.png", local.ThumbnailUrl);
        Assert.Equal("https://cdn/banner.png", local.ImageUrl);
        Assert.Equal(timestamp, local.Timestamp);
        Assert.True(local.Author.HasValue);
        Assert.Equal("Hanekawa", local.Author.Value.Name);
        Assert.True(local.Footer.HasValue);
        Assert.Equal("Updated", local.Footer.Value.Text);
        Assert.True(local.Fields.HasValue);
        Assert.Equal(2, local.Fields.Value.Count);
        Assert.Equal("XP", local.Fields.Value[0].Name);
        Assert.True(local.Fields.Value[0].IsInline.Value);
    }

    [Fact]
    public void ToLocalEmbed_NoFields_OmitsFieldsCollection()
    {
        var local = new Embed { Title = "Ping", Content = "pong" }.ToLocalEmbed();

        Assert.False(local.Fields.HasValue);
    }

    [Fact]
    public void ToLocalEmbed_NullTitleAndContent_MapToEmptyStrings()
    {
        var local = new Embed { Color = 0xFF0000 }.ToLocalEmbed();

        Assert.Equal(string.Empty, local.Title);
        Assert.Equal(string.Empty, local.Description);
        Assert.Equal(string.Empty, local.ThumbnailUrl);
        Assert.Equal(string.Empty, local.ImageUrl);
    }

    [Fact]
    public void ToLocalInteractionMessageResponse_TextOnly_DoesNotThrow()
    {
        var response = new Response<Message>(new Message("banned user"));

        var local = response.ToLocalInteractionMessageResponse();

        Assert.Equal("banned user", local.Content.Value);
        Assert.False(local.Embeds.HasValue);
    }

    [Fact]
    public void ToLocalInteractionMessageResponse_WithEmbed_MapsEmbed()
    {
        var response = new Response<Message>(new Message(new Embed
        {
            Title = "Boost",
            Content = "config"
        }));

        var local = response.ToLocalInteractionMessageResponse();

        Assert.True(local.Embeds.HasValue);
        Assert.Equal("Boost", local.Embeds.Value[0].Title.Value);
    }

    [Fact]
    public void ToLocalInteractionMessageResponse_MapsEphemeralAndAllowedMentions()
    {
        var withMentions = new Response<Message>(new Message("secret", allowMentions: true, ephemeral: false))
            .ToLocalInteractionMessageResponse();
        var withoutMentions = new Response<Message>(new Message("secret", allowMentions: false, ephemeral: false))
            .ToLocalInteractionMessageResponse();

        Assert.False(withMentions.IsEphemeral);
        Assert.False(withoutMentions.IsEphemeral);
        Assert.NotEqual(withMentions.AllowedMentions, withoutMentions.AllowedMentions);
    }

    [Fact]
    public void ToLocalInteractionMessageResponse_DefaultMessage_IsEphemeral()
    {
        var local = new Response<Message>(new Message("done")).ToLocalInteractionMessageResponse();

        Assert.True(local.IsEphemeral);
    }

    [Fact]
    public void ToPages_WithTwoItems_DoesNotThrow()
    {
        var response = new Response<Pagination<Message>>(new Pagination<Message>(
        [
            new Message("page 1"),
            new Message("page 2")
        ]));

        var pages = response.ToPages();

        Assert.Equal(2, pages.Length);
        Assert.Equal("page 1", pages[0].Content.Value);
        Assert.Equal("page 2", pages[1].Content.Value);
    }

    [Fact]
    public void ToPages_WithNoItems_ReturnsEmptyArray()
    {
        var pages = new Response<Pagination<Message>>(new Pagination<Message>([])).ToPages();

        Assert.Empty(pages);
    }
}
