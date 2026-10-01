using EntityFrameworkCore.Triggered;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Infrastructure.Triggers;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests.Infrastructure;

public class ModLogBeforeTriggerTests
{
    [Fact]
    public async Task BeforeSave_DoesNotAssignId_WhenChangeIsNotAdded()
    {
        var entity = new GuildModerationLog { GuildId = 1, Id = 0 };
        var (sut, _) = CreateSut([]);
        var context = CreateContext(entity, ChangeType.Modified);

        await sut.BeforeSave(context, CancellationToken.None);

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public async Task BeforeSave_AssignsSequentialId_PerGuild()
    {
        var logs = new List<GuildModerationLog>
        {
            new() { GuildId = 1, Id = 1 },
            new() { GuildId = 1, Id = 2 },
            new() { GuildId = 2, Id = 5 }
        };
        var entity = new GuildModerationLog { GuildId = 1, Id = 0 };
        var (sut, _) = CreateSut(logs);
        var context = CreateContext(entity, ChangeType.Added);

        await sut.BeforeSave(context, CancellationToken.None);

        Assert.Equal(3, entity.Id);
    }

    [Fact]
    public async Task BeforeSave_StartsAtOne_WhenGuildHasNoLogs()
    {
        var entity = new GuildModerationLog { GuildId = 9, Id = 0 };
        var (sut, _) = CreateSut([]);
        var context = CreateContext(entity, ChangeType.Added);

        await sut.BeforeSave(context, CancellationToken.None);

        Assert.Equal(1, entity.Id);
    }

    private static (TestModLogBeforeTrigger Sut, Mock<IDbContext> Db) CreateSut(List<GuildModerationLog> logs)
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.ModerationLogs).Returns(logs.MockDbSet().Object);
        var sut = new TestModLogBeforeTrigger(db.Object);
        return (sut, db);
    }

    private static ITriggerContext<GuildModerationLog> CreateContext(GuildModerationLog entity, ChangeType changeType)
    {
        var context = new Mock<ITriggerContext<GuildModerationLog>>();
        context.Setup(x => x.Entity).Returns(entity);
        context.Setup(x => x.ChangeType).Returns(changeType);
        return context.Object;
    }

    private sealed class TestModLogBeforeTrigger(IDbContext db) : ModLogBeforeTrigger(db);
}
