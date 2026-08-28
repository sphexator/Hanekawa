using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class DropServiceTests
{
    private const ulong GuildId = 1;
    private const ulong ChannelId = 5;
    private const ulong MessageId = 99;
    private const ulong UserId = 10;

    [Fact]
    public async Task DropAsync_DoesNotTrigger_WhenRandomRollIsBelowThreshold()
    {
        var (sut, bot, cache, _) = CreateSut([], new FixedRandom(849));

        await sut.DropAsync(CreateChannel(), CreateMember());

        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<Attachment>()), Times.Never);
        cache.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task DropAsync_DoesNotTrigger_WhenGuildConfigIsMissing()
    {
        var (sut, bot, cache, _) = CreateSut([], new FixedRandom(900));

        await sut.DropAsync(CreateChannel(), CreateMember());

        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<Attachment>()), Times.Never);
        cache.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task DropAsync_DoesNotTrigger_WhenChannelIsBlacklisted()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                DropConfig = new DropConfig { GuildId = GuildId, Blacklist = [ChannelId] }
            }
        };
        var (sut, bot, cache, _) = CreateSut(configs, new FixedRandom(900));

        await sut.DropAsync(CreateChannel(), CreateMember());

        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<Attachment>()), Times.Never);
        cache.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task DropAsync_SendsMessageAndCachesClaimKey_WhenTriggered()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                DropConfig = new DropConfig { GuildId = GuildId, Emote = "⭐", ExpReward = 50 }
            }
        };
        var (sut, bot, cache, _) = CreateSut(configs, new FixedRandom(900));
        bot.Setup(x => x.SendMessageAsync(ChannelId, It.IsAny<string>(), It.IsAny<Attachment>()))
            .ReturnsAsync(new RestMessage { Id = MessageId, ChannelId = ChannelId });

        await sut.DropAsync(CreateChannel(), CreateMember());

        bot.Verify(x => x.SendMessageAsync(ChannelId, It.Is<string>(m => m.Contains("drop event")), It.IsAny<Attachment>()),
            Times.Once);
        cache.Verify(x => x.Add($"{ChannelId}-{MessageId}-drop", UserId), Times.Once);
    }

    [Fact]
    public async Task ClaimAsync_DoesNotReward_WhenDropIsNotCached()
    {
        var (sut, bot, cache, levels) = CreateSut([], new FixedRandom(0));
        cache.Setup(x => x.Get<GuildUser>(It.IsAny<string>())).Returns((GuildUser?)null);

        await sut.ClaimAsync(ChannelId, MessageId, CreateMember());

        bot.Verify(x => x.DeleteMessageAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        levels.Verify(x => x.AddExperienceAsync(It.IsAny<DiscordMember>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ClaimAsync_DeletesMessageAwardsExperienceAndClearsCache()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                DropConfig = new DropConfig { GuildId = GuildId, ExpReward = 50 }
            }
        };
        var (sut, bot, cache, levels) = CreateSut(configs, new FixedRandom(0));
        cache.Setup(x => x.Get<GuildUser>($"{MessageId}-{ChannelId}-drop"))
            .Returns(new GuildUser { Id = UserId, GuildId = GuildId });
        levels.Setup(x => x.AddExperienceAsync(It.IsAny<DiscordMember>(), 50)).ReturnsAsync(50);
        bot.Setup(x => x.DeleteMessageAsync(GuildId, ChannelId, MessageId)).Returns(Task.CompletedTask);
        bot.Setup(x => x.SendMessageAsync(ChannelId, It.IsAny<string>(), It.IsAny<Attachment>()))
            .ReturnsAsync(new RestMessage());

        var member = CreateMember();
        await sut.ClaimAsync(ChannelId, MessageId, member);

        bot.Verify(x => x.DeleteMessageAsync(GuildId, ChannelId, MessageId), Times.Once);
        levels.Verify(x => x.AddExperienceAsync(member, 50), Times.Once);
        bot.Verify(x => x.SendMessageAsync(ChannelId, It.Is<string>(m => m.Contains("50 experience")), It.IsAny<Attachment>()),
            Times.Once);
        cache.Verify(x => x.Remove($"{MessageId}-{ChannelId}-drop"), Times.Once);
    }

    private static (DropService Sut, Mock<IBot> Bot, Mock<ICacheContext> Cache, Mock<ILevelService> Levels)
        CreateSut(List<GuildConfig> configs, Random random)
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var bot = new Mock<IBot>();
        var cache = new Mock<ICacheContext>();
        var levels = new Mock<ILevelService>();

        var services = new ServiceCollection();
        services.AddSingleton(db.Object);
        services.AddSingleton(bot.Object);
        services.AddSingleton(cache.Object);
        var provider = services.BuildServiceProvider();

        var sut = new DropService(levels.Object, new Mock<ILogger<DropService>>().Object, provider, random);
        return (sut, bot, cache, levels);
    }

    private static TextChannel CreateChannel()
        => new() { Id = ChannelId, Name = "drops", GuildId = GuildId, Mention = $"<#{ChannelId}>" };

    private static DiscordMember CreateMember()
        => new()
        {
            Id = UserId,
            Username = "dropper",
            Nickname = "Dropper",
            Guild = new Guild { GuildId = GuildId, Name = "guild", Emotes = [] }
        };

    private sealed class FixedRandom(int next) : Random
    {
        public override int Next() => next;
        public override int Next(int maxValue) => next;
        public override int Next(int minValue, int maxValue) => next;
    }
}
