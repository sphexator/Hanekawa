using Hanekawa.Application.Handlers.Commands.Account;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class AccountCommandServiceTests
{
    private readonly DiscordMember _member = new()
    {
        Id = 10,
        Username = "user",
        Guild = new Guild { GuildId = 1, Name = "guild" }
    };

    [Fact]
    public async Task GetWalletAsync_ReturnsExistingUserCurrency()
    {
        var users = new List<GuildUser>
        {
            new()
            {
                GuildId = 1,
                Id = 10,
                Currency = 250,
                User = new User { Id = 10 }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).ReturnsDbSet(users);

        var sut = new AccountCommandService(Mock.Of<IImageService>(), db.Object);

        var currency = await sut.GetWalletAsync(_member);

        Assert.Equal(250, currency);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopUsersAsync_ReturnsActiveUsersOrderedByExperience()
    {
        var users = new List<GuildUser>
        {
            new() { GuildId = 1, Id = 1, Experience = 100, Inactive = false },
            new() { GuildId = 1, Id = 2, Experience = 500, Inactive = false },
            new() { GuildId = 1, Id = 3, Experience = 999, Inactive = true },
            new() { GuildId = 2, Id = 4, Experience = 800, Inactive = false },
            new() { GuildId = 1, Id = 5, Experience = 200, Inactive = false }
        };
        for (ulong i = 10; i <= 20; i++)
        {
            users.Add(new GuildUser { GuildId = 1, Id = i, Experience = (long)i, Inactive = false });
        }

        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).ReturnsDbSet(users);

        var sut = new AccountCommandService(Mock.Of<IImageService>(), db.Object);

        var top = await sut.GetTopUsersAsync(1);

        Assert.Equal(10, top.Length);
        Assert.Equal(2ul, top[0].Id);
        Assert.DoesNotContain(top, x => x.Inactive);
        Assert.DoesNotContain(top, x => x.GuildId != 1);
        Assert.Equal(top.OrderByDescending(x => x.Experience).Select(x => x.Id), top.Select(x => x.Id));
    }
}
