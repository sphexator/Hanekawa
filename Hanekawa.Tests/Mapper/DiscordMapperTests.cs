using Disqord;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Tests.Mapper;

public class DiscordMapperTests
{
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
}
