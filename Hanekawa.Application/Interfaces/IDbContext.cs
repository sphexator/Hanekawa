﻿using System.Data.Common;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Club;
using Hanekawa.Entities.Internals;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Interfaces;

/// <inheritdoc />
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
    /// Guild moderator store. Ban / Mute / etc
    /// </summary>
    DbSet<GuildModerationLog> ModerationLogs { get; set; }
    /// <summary>
    /// Club store
    /// </summary>
    DbSet<Club> Clubs { get; set; }
    /// <summary>
    /// Club members store
    /// </summary>
    DbSet<ClubMember> ClubMembers { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> EnsureDatabaseCreated(CancellationToken cancellationToken = default);
    Task MigrateDatabaseAsync(CancellationToken cancellationToken = default);
    DbConnection GetConnection();
    Task<T?> ExecuteQuery<T>(string query, object? param = null, CancellationToken cancellationToken = default);
}