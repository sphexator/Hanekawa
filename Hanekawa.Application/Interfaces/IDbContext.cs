using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Internals;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Interfaces;

/// <summary>
/// Database context interface
/// </summary>
public interface IDbContext : IAsyncDisposable
{
    /// <summary>
    /// Guild configuration store
    /// </summary>
    DbSet<GuildConfig> GuildConfigs { get; set; }
    /// <summary>
    /// User store
    /// </summary>
    DbSet<GuildUser> Users { get; set; }
    /// <summary>
    /// Level requirement between each level
    /// </summary>
    DbSet<LevelRequirement> LevelRequirements { get; set; }
    /// <summary>
    /// Warning store
    /// </summary>
    DbSet<Warning> Warnings { get; set; }
    /// <summary>
    /// Logging store
    /// </summary>
    DbSet<Log> Logs { get; set; }

    /// <summary>
    /// Saves changes in current context
    /// </summary>
    /// <returns></returns>
    Task<int> SaveChangesAsync();
    /// <summary>
    /// Checks if the database is created
    /// </summary>
    /// <returns></returns>
    Task<bool> EnsureDatabaseCreated();
    /// <summary>
    /// Migrates pending migrations to the database
    /// </summary>
    /// <returns></returns>
    Task MigrateDatabaseAsync();
}