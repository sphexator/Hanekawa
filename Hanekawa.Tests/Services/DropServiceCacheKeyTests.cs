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

public class DropServiceCacheKeyTests
{
    private const ulong GuildId = 1;
    private const ulong ChannelId = 5;
    private const ulong MessageId = 99;
    private const ulong UserId = 10;

    [Fact]
    public async Task ClaimAsync_RewardsDrop_AfterDropAsyncStoresMatchingCacheEntry()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                DropConfig = new DropConfig { GuildId = GuildId, Emote = "⭐", ExpReward = 25 }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);

        var bot = new Mock<IBot>();
        bot.Setup(x => x.SendMessageAsync(ChannelId, It.IsAny<string>(), It.IsAny<Attachment>()))
            .ReturnsAsync(new RestMessage { Id = MessageId, ChannelId = ChannelId });
        bot.Setup(x => x.DeleteMessageAsync(GuildId, ChannelId, MessageId)).Returns(Task.CompletedTask);

        var cache = new InMemoryCacheContext();
        var levels = new Mock<ILevelService>();
        levels.Setup(x => x.AddExperienceAsync(It.IsAny<DiscordMember>(), 25)).ReturnsAsync(25);

        var services = new ServiceCollection();
        services.AddSingleton(db.Object);
        services.AddSingleton(bot.Object);
        services.AddSingleton<ICacheContext>(cache);
        services.AddSingleton(levels.Object);
        var provider = services.BuildServiceProvider();

        var member = new DiscordMember
        {
            Id = UserId,
            Username = "dropper",
            Guild = new Guild { GuildId = GuildId, Name = "guild", Emotes = [] }
        };
        var sut = new DropService(levels.Object, Mock.Of<ILogger<DropService>>(), provider, new FixedRandom(900));

        await sut.DropAsync(new TextChannel { Id = ChannelId, GuildId = GuildId, Name = "drops" }, member);
        await sut.ClaimAsync(ChannelId, MessageId, member);

        levels.Verify(x => x.AddExperienceAsync(member, 25), Times.Once);
        Assert.False(cache.Contains($"{MessageId}-{ChannelId}-drop"));
    }

    private sealed class InMemoryCacheContext : ICacheContext
    {
        private readonly Dictionary<string, object> _store = new();

        public TEntity? Get<TEntity>(string key)
            => _store.TryGetValue(key, out var value) ? (TEntity)value : default;

        public TEntity? Get<TEntity>(string key, TimeSpan expiration) => Get<TEntity>(key);

        public void Add<TEntity>(string key, TEntity value) => _store[key] = value!;

        public void Add<TEntity>(string key, TEntity value, TimeSpan expiration) => Add(key, value);

        public bool Remove(string key) => _store.Remove(key);

        public ValueTask<TEntity> GetOrCreateAsync<TEntity>(string key, Func<Task<TEntity>> factory)
            => ValueTask.FromResult(Get<TEntity>(key) ?? factory().Result);

        public bool Contains(string key) => _store.ContainsKey(key);
    }

    private sealed class FixedRandom(int next) : Random
    {
        public override int Next(int maxValue) => next;
        public override int Next(int minValue, int maxValue) => next;
    }
}
