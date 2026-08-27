using System.Text.Json;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Entities;
using Microsoft.Extensions.Caching.Distributed;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class ModuleServiceTests
{
    [Fact]
    public async Task IsEnabledAsync_ReturnsFalse_WhenNoRowExists()
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(new List<Module>());
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        var result = await sut.IsEnabledAsync(1, ModuleName.Level);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsTrue_WhenModuleEnabled()
    {
        var modules = new List<Module>
        {
            new() { GuildId = 1, Name = ModuleName.Level, Enabled = true }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(modules);
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        var result = await sut.IsEnabledAsync(1, ModuleName.Level);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsFalse_WhenModuleDisabled()
    {
        var modules = new List<Module>
        {
            new() { GuildId = 1, Name = ModuleName.Level, Enabled = false }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(modules);
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        var result = await sut.IsEnabledAsync(1, ModuleName.Level);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_UsesCachedState_WhenPresent()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync("1-Modules", It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.SerializeToUtf8Bytes(
                new Dictionary<string, bool> { [ModuleName.Club] = true }));
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(new List<Module>());
        var sut = new ModuleService(cache.Object, db.Object);

        var result = await sut.IsEnabledAsync(1, ModuleName.Club);

        Assert.True(result);
    }

    [Fact]
    public async Task SetEnabledAsync_InsertsRow_WhenMissing()
    {
        var modules = new List<Module>();
        var dbSet = modules.BuildMockDbSet();
        dbSet.Setup(x => x.AddAsync(It.IsAny<Module>(), It.IsAny<CancellationToken>()))
            .Callback<Module, CancellationToken>((m, _) => modules.Add(m))
            .ReturnsAsync((Module m, CancellationToken _) => null!);
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).Returns(dbSet.Object);
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        await sut.SetEnabledAsync(1, ModuleName.Boost, true);

        var row = Assert.Single(modules);
        Assert.Equal(ModuleName.Boost, row.Name);
        Assert.True(row.Enabled);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetEnabledAsync_UpdatesRow_WhenPresent()
    {
        var existing = new Module { GuildId = 1, Name = ModuleName.Boost, Enabled = false };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(new List<Module> { existing });
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        await sut.SetEnabledAsync(1, ModuleName.Boost, true);

        Assert.True(existing.Enabled);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModulesAsync_ReturnsAllKnownModules_WithStoredState()
    {
        var modules = new List<Module>
        {
            new() { GuildId = 1, Name = ModuleName.Level, Enabled = true }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Modules).ReturnsDbSet(modules);
        var sut = new ModuleService(new Mock<IDistributedCache>().Object, db.Object);

        var result = await sut.GetModulesAsync(1);

        Assert.Equal(ModuleName.All.Length, result.Count);
        Assert.True(result.Single(x => x.Name == ModuleName.Level).Enabled);
        Assert.False(result.Single(x => x.Name == ModuleName.Club).Enabled);
    }
}
