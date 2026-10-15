using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Hanekawa.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Tests.Extensions;

/// <summary>
/// Covers the duplicate-key recovery in <see cref="DbExtensions.GetOrCreateUserAsync"/>.
/// A failed insert must be detached so a later save updates the row another instance inserted
/// instead of retrying the insert.
/// </summary>
public sealed class GetOrCreateUserDuplicateKeyTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        await using var db = new DbService(Options());
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public async Task GetOrCreateUserAsync_AfterDuplicateKey_PersistsExperienceOnTheExistingRow()
    {
        await using var db = new DuplicateKeyDb(_connection);

        var user = await db.GetOrCreateUserAsync(2, 10);
        user.Experience = 40;
        await db.SaveChangesAsync();

        await using var verify = new DbService(Options());
        var stored = await verify.Users.SingleAsync(x => x.GuildId == 2 && x.Id == 10);
        Assert.Equal(40, stored.Experience);
        Assert.Equal(1, await verify.Users.CountAsync());
    }

    private DbContextOptions<DbService> Options()
        => new DbContextOptionsBuilder<DbService>().UseSqlite(_connection).Options;

    /// <summary>
    /// The first save throws after another context inserts the same guild user, matching a
    /// cross-process unique-key race. Later saves go to SQLite.
    /// </summary>
    private sealed class DuplicateKeyDb : DbService, IDbContext
    {
        private readonly SqliteConnection _connection;
        private bool _throwOnNextSave = true;

        public DuplicateKeyDb(SqliteConnection connection)
            : base(new DbContextOptionsBuilder<DbService>().UseSqlite(connection).Options)
        {
            _connection = connection;
        }

        public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_throwOnNextSave)
            {
                _throwOnNextSave = false;
                await using var other = new DbService(
                    new DbContextOptionsBuilder<DbService>().UseSqlite(_connection).Options);
                await other.Users.AddAsync(new GuildUser
                {
                    GuildId = 2,
                    Id = 10,
                    User = new User { Id = 10 }
                }, cancellationToken);
                await other.SaveChangesAsync(cancellationToken);
                throw new DbUpdateException("Duplicate key");
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
