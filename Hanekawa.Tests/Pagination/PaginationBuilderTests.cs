using Hanekawa.Entities;
using Hanekawa.Extensions;

namespace Hanekawa.Tests.Pagination;

public class PaginationBuilderTests
{
    [Fact]
    public void Paginate_WithTwoPages_MapsEachPageWithoutThrowing()
    {
        var warnings = Enumerable.Range(0, 6)
            .Select(i => $"warning-{i}")
            .ToList();

        var pages = warnings.BuildPage().Paginate<Message>();

        Assert.Equal(2, pages.Length);
        Assert.Contains("warning-0", pages[0].Content);
        Assert.Contains("warning-5", pages[1].Content);
    }

    [Fact]
    public void Paginate_WithSinglePage_ReturnsOneMessage()
    {
        var warnings = Enumerable.Range(0, 3)
            .Select(i => $"warning-{i}")
            .ToList();

        var pages = warnings.BuildPage().Paginate<Message>();

        Assert.Single(pages);
        Assert.Contains("warning-0", pages[0].Content);
        Assert.Contains("warning-2", pages[0].Content);
    }
}
