using Hanekawa.Bot.Commands.Slash.Administration;

namespace Hanekawa.Tests.Bot;

public class PruneSelectionTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SelectDeletable_KeepsMessagesYoungerThan14Days()
    {
        var messages = new (ulong Id, DateTimeOffset CreatedAt)[]
        {
            (10, Now.AddDays(-1)),
            (11, Now.AddHours(-1))
        };

        var ids = PruneSelection.SelectDeletable(messages, Now);

        Assert.Equal(new ulong[] { 10, 11 }, ids);
    }

    [Fact]
    public void SelectDeletable_DropsOldMessagesAndZeroIdsWithoutLeavingHoles()
    {
        var messages = new (ulong Id, DateTimeOffset CreatedAt)[]
        {
            (10, Now.AddDays(-1)),
            (20, Now.AddDays(-15)),
            (0, Now.AddHours(-1)),
            (30, Now.AddDays(-13))
        };

        var ids = PruneSelection.SelectDeletable(messages, Now);

        Assert.Equal(new ulong[] { 10, 30 }, ids);
    }

    [Fact]
    public void SelectDeletable_ReturnsEmpty_WhenEveryMessageIsTooOld()
    {
        var messages = new (ulong Id, DateTimeOffset CreatedAt)[]
        {
            (10, Now.AddDays(-14).AddTicks(-1)),
            (11, Now.AddDays(-30))
        };

        var ids = PruneSelection.SelectDeletable(messages, Now);

        Assert.Empty(ids);
    }
}
